using Microsoft.EntityFrameworkCore;
using TestFirstProject.Contexts;
using TestFirstProject.Exceptions;
using TestFirstProject.Models.Game;
using TestFirstProject.Models.Game.Enums;
using TestFirstProject.Services.Implementations.Game.EnemyAI;
using TestFirstProject.Services.Interfaces.Game;

namespace TestFirstProject.Services.Implementations.Game
{
    /// <summary>
    /// Core combat engine: manages the full turn-based battle loop.
    /// Turn order by Speed (with Haste/Slow modifiers). All state persisted in DB.
    /// </summary>
    public class CombatEngine : ICombatEngine
    {
        private readonly GameDbContext _db;
        private readonly IDamageCalculator _damageCalc;
        private readonly IStatusEffectProcessor _statusProcessor;
        private readonly ILootService _lootService;
        private readonly ICharacterProgressionService _progression;

        public CombatEngine(
            GameDbContext db,
            IDamageCalculator damageCalc,
            IStatusEffectProcessor statusProcessor,
            ILootService lootService,
            ICharacterProgressionService progression)
        {
            _db = db;
            _damageCalc = damageCalc;
            _statusProcessor = statusProcessor;
            _lootService = lootService;
            _progression = progression;
        }

        public async Task<Battle> StartBattleAsync(Guid characterId, Guid enemyId, Guid? dungeonRoomId = null)
        {
            var character = await _db.Characters
                .Include(c => c.Stats)
                .Include(c => c.EquippedItems).ThenInclude(ei => ei.InventoryItem).ThenInclude(ii => ii.Item)
                .Include(c => c.LearnedSkills).ThenInclude(ls => ls.Skill)
                .FirstOrDefaultAsync(c => c.Id == characterId)
                ?? throw new NotFoundException("Character not found.");

            // Check no active battle
            var activeBattle = await _db.Battles
                .FirstOrDefaultAsync(b => b.CharacterId == characterId && b.Status == BattleStatus.InProgress);
            if (activeBattle != null)
                throw new ConflictException("Character already has an active battle.");

            var enemy = await _db.Enemies
                .Include(e => e.Skills)
                .FirstOrDefaultAsync(e => e.Id == enemyId)
                ?? throw new NotFoundException("Enemy not found.");

            // Calculate total stats for the character
            var totalStats = await _progression.GetTotalStatsAsync(characterId);

            var battle = new Battle
            {
                Id = Guid.NewGuid(),
                CharacterId = characterId,
                Status = BattleStatus.InProgress,
                CurrentTurn = 1,
                DungeonRoomId = dungeonRoomId
            };

            _db.Battles.Add(battle);

            // Create player participant
            var playerParticipant = new BattleParticipant
            {
                Id = Guid.NewGuid(),
                BattleId = battle.Id,
                Type = ParticipantType.Player,
                CharacterId = characterId,
                Name = character.Name,
                CurrentHp = character.CurrentHp,
                MaxHp = totalStats.BaseHp,
                CurrentMp = character.CurrentMp,
                MaxMp = totalStats.BaseMp,
                Attack = totalStats.BaseAttack,
                Defense = totalStats.BaseDefense,
                MagicPower = totalStats.BaseMagicPower,
                Speed = totalStats.BaseSpeed,
                CritChance = totalStats.BaseCritChance,
                DodgeChance = totalStats.BaseDodgeChance
            };

            // Create enemy participant (stats from enemy template)
            var enemyParticipant = new BattleParticipant
            {
                Id = Guid.NewGuid(),
                BattleId = battle.Id,
                Type = ParticipantType.Enemy,
                EnemyId = enemyId,
                Name = enemy.Name,
                CurrentHp = enemy.BaseHp,
                MaxHp = enemy.BaseHp,
                CurrentMp = enemy.BaseMp,
                MaxMp = enemy.BaseMp,
                Attack = enemy.BaseAttack,
                Defense = enemy.BaseDefense,
                MagicPower = enemy.BaseMagicPower,
                Speed = enemy.BaseSpeed,
                CritChance = enemy.BaseCritChance,
                DodgeChance = enemy.BaseDodgeChance
            };

            // Set turn order by speed (lower index = goes first)
            var participants = new List<BattleParticipant> { playerParticipant, enemyParticipant };
            var ordered = participants.OrderByDescending(p => p.Speed).ToList();
            for (int i = 0; i < ordered.Count; i++)
                ordered[i].TurnOrder = i;

            _db.BattleParticipants.AddRange(participants);
            await _db.SaveChangesAsync();

            return await GetBattleStateAsync(battle.Id);
        }

        public async Task<Battle> GetBattleStateAsync(Guid battleId)
        {
            var battle = await _db.Battles
                .Include(b => b.Participants)
                .Include(b => b.StatusEffects)
                .FirstOrDefaultAsync(b => b.Id == battleId)
                ?? throw new NotFoundException("Battle not found.");

            return battle;
        }

        public async Task<BattleTurnResult> SubmitActionAsync(Guid battleId, Guid playerId, BattleActionType actionType, Guid? skillId = null, Guid? itemId = null, Guid? targetId = null)
        {
            var battle = await _db.Battles
                .Include(b => b.Participants)
                .Include(b => b.StatusEffects)
                .FirstOrDefaultAsync(b => b.Id == battleId)
                ?? throw new NotFoundException("Battle not found.");

            if (battle.Status != BattleStatus.InProgress)
                throw new ValidationException("Battle is not in progress.");

            // Verify the character belongs to this player
            var character = await _db.Characters
                .FirstOrDefaultAsync(c => c.Id == battle.CharacterId && c.PlayerId == playerId)
                ?? throw new ForbiddenException("This battle does not belong to you.");

            var result = new BattleTurnResult
            {
                BattleId = battleId,
                TurnNumber = battle.CurrentTurn
            };

            // Get ordered participants
            var orderedParticipants = battle.Participants
                .Where(p => p.IsAlive)
                .OrderBy(p => p.TurnOrder)
                .ToList();

            // Apply Haste/Slow modifiers to effective speed for turn ordering
            foreach (var p in orderedParticipants)
            {
                var hasHaste = battle.StatusEffects.Any(e => e.TargetParticipantId == p.Id && e.Type == StatusEffectType.Haste);
                var hasSlow = battle.StatusEffects.Any(e => e.TargetParticipantId == p.Id && e.Type == StatusEffectType.Slow);
                var effectiveSpeed = p.Speed;
                if (hasHaste) effectiveSpeed = (int)(effectiveSpeed * 1.5);
                if (hasSlow) effectiveSpeed = (int)(effectiveSpeed * 0.6);
                p.TurnOrder = -effectiveSpeed; // Negative so higher speed = lower order number
            }
            orderedParticipants = orderedParticipants.OrderBy(p => p.TurnOrder).ToList();

            // Process each participant's turn
            foreach (var participant in orderedParticipants)
            {
                if (!participant.IsAlive) continue;

                // Process status effects at turn start
                var effectLog = await _statusProcessor.ProcessTurnStartEffectsAsync(battleId, participant);
                result.Log.AddRange(effectLog);

                // Check if participant died from DoT
                if (!participant.IsAlive)
                {
                    result.Log.Add(new BattleLogEntry
                    {
                        Actor = participant.Name,
                        Action = "Defeated",
                        Message = $"{participant.Name} has been defeated by status effects!"
                    });
                    continue;
                }

                // Check stun
                if (await _statusProcessor.IsStunnedAsync(battleId, participant.Id))
                {
                    await _statusProcessor.TickEffectsAsync(battleId, participant.Id);
                    continue; // Skip turn
                }

                if (participant.Type == ParticipantType.Player)
                {
                    // Process player action
                    await ProcessPlayerAction(battle, participant, actionType, skillId, itemId, targetId, result);
                }
                else
                {
                    // Process enemy AI action
                    await ProcessEnemyAction(battle, participant, result);
                }

                // Tick effects (decrement durations)
                await _statusProcessor.TickEffectsAsync(battleId, participant.Id);

                // Check battle end conditions
                var battleEnd = CheckBattleEnd(battle);
                if (battleEnd != null)
                {
                    battle.Status = battleEnd.Value;
                    battle.CompletedAt = DateTime.UtcNow;

                    if (battle.Status == BattleStatus.Victory)
                    {
                        await HandleVictory(battle, character, result);
                    }
                    else if (battle.Status == BattleStatus.Defeat)
                    {
                        result.Log.Add(new BattleLogEntry
                        {
                            Actor = "System",
                            Action = "Defeat",
                            Message = "Your character has been defeated!"
                        });
                    }

                    result.Status = battle.Status;
                    await SyncCharacterState(battle);
                    await _db.SaveChangesAsync();
                    return result;
                }
            }

            // Advance turn
            battle.CurrentTurn++;
            result.Status = BattleStatus.InProgress;

            await SyncCharacterState(battle);
            await _db.SaveChangesAsync();
            return result;
        }

        private async Task ProcessPlayerAction(Battle battle, BattleParticipant player, BattleActionType actionType, Guid? skillId, Guid? itemId, Guid? targetId, BattleTurnResult result)
        {
            var enemies = battle.Participants.Where(p => p.Type == ParticipantType.Enemy && p.IsAlive).ToList();
            var target = targetId.HasValue
                ? battle.Participants.FirstOrDefault(p => p.Id == targetId.Value && p.IsAlive)
                : enemies.FirstOrDefault();

            switch (actionType)
            {
                case BattleActionType.Attack:
                    if (target == null) break;
                    await ProcessBasicAttack(battle, player, target, result);
                    break;

                case BattleActionType.UseSkill:
                    if (!skillId.HasValue)
                        throw new ValidationException("Skill ID is required for UseSkill action.");
                    await ProcessSkillUse(battle, player, skillId.Value, target, result);
                    break;

                case BattleActionType.UseItem:
                    if (!itemId.HasValue)
                        throw new ValidationException("Item ID is required for UseItem action.");
                    await ProcessItemUse(battle, player, itemId.Value, result);
                    break;

                case BattleActionType.Flee:
                    await ProcessFlee(battle, player, result);
                    break;
            }
        }

        private async Task ProcessBasicAttack(Battle battle, BattleParticipant attacker, BattleParticipant target, BattleTurnResult result)
        {
            var dmgResult = _damageCalc.CalculateBasicAttack(attacker, target);

            if (dmgResult.IsDodged)
            {
                result.Log.Add(new BattleLogEntry
                {
                    Actor = attacker.Name,
                    Action = "Attack",
                    Target = target.Name,
                    IsDodged = true,
                    Message = $"{target.Name} dodged {attacker.Name}'s attack!"
                });
                return;
            }

            // Apply shield absorption
            var actualDamage = await ApplyShieldAbsorption(battle.Id, target.Id, dmgResult.Damage);

            target.CurrentHp = Math.Max(target.CurrentHp - actualDamage, 0);

            result.Log.Add(new BattleLogEntry
            {
                Actor = attacker.Name,
                Action = "Attack",
                Target = target.Name,
                Damage = actualDamage,
                IsCritical = dmgResult.IsCritical,
                Message = $"{attacker.Name} attacks {target.Name} for {actualDamage} damage.{(dmgResult.IsCritical ? " Critical hit!" : "")}"
            });

            // Check boss phase transition
            await CheckBossPhaseTransition(battle, target, result);
        }

        private async Task ProcessSkillUse(Battle battle, BattleParticipant user, Guid skillId, BattleParticipant? target, BattleTurnResult result)
        {
            var learnedSkill = await _db.LearnedSkills
                .Include(ls => ls.Skill)
                .FirstOrDefaultAsync(ls => ls.CharacterId == battle.CharacterId && ls.SkillId == skillId);

            if (learnedSkill == null)
                throw new ValidationException("Skill not learned.");

            var skill = learnedSkill.Skill;

            if (skill.Type != SkillType.Active)
                throw new ValidationException("Cannot use passive skills in combat.");

            if (learnedSkill.CurrentCooldown > 0)
                throw new ValidationException($"Skill is on cooldown ({learnedSkill.CurrentCooldown} turns remaining).");

            if (user.CurrentMp < skill.MpCost)
                throw new ValidationException("Not enough MP.");

            // Consume MP
            user.CurrentMp -= skill.MpCost;

            // Set cooldown
            learnedSkill.CurrentCooldown = skill.CooldownTurns;

            // Handle different target types
            var targets = new List<BattleParticipant>();
            if (skill.TargetType == SkillTargetType.Self)
            {
                targets.Add(user);
            }
            else if (skill.TargetType == SkillTargetType.All)
            {
                targets.AddRange(battle.Participants.Where(p =>
                    p.Type == (user.Type == ParticipantType.Player ? ParticipantType.Enemy : ParticipantType.Player)
                    && p.IsAlive));
            }
            else
            {
                if (target != null) targets.Add(target);
            }

            foreach (var t in targets)
            {
                if (skill.BaseDamage > 0 || skill.DamageMultiplier > 0)
                {
                    if (skill.TargetType != SkillTargetType.Self)
                    {
                        var dmgResult = _damageCalc.CalculateSkillDamage(user, t, skill);

                        if (dmgResult.IsDodged)
                        {
                            result.Log.Add(new BattleLogEntry
                            {
                                Actor = user.Name,
                                Action = skill.Name,
                                Target = t.Name,
                                IsDodged = true,
                                Message = $"{t.Name} dodged {user.Name}'s {skill.Name}!"
                            });
                            continue;
                        }

                        var actualDamage = await ApplyShieldAbsorption(battle.Id, t.Id, dmgResult.Damage);
                        t.CurrentHp = Math.Max(t.CurrentHp - actualDamage, 0);

                        result.Log.Add(new BattleLogEntry
                        {
                            Actor = user.Name,
                            Action = skill.Name,
                            Target = t.Name,
                            Damage = actualDamage,
                            IsCritical = dmgResult.IsCritical,
                            Message = $"{user.Name} uses {skill.Name} on {t.Name} for {actualDamage} damage.{(dmgResult.IsCritical ? " Critical hit!" : "")}"
                        });
                    }
                }

                // Apply status effect if the skill has one
                if (skill.AppliesEffect.HasValue)
                {
                    var effectTarget = skill.TargetType == SkillTargetType.Self ? user : t;
                    await _statusProcessor.ApplyEffectAsync(
                        battle.Id, effectTarget.Id,
                        skill.AppliesEffect.Value,
                        skill.EffectDuration,
                        skill.EffectTickValue,
                        skill.EffectStackLimit);

                    result.Log.Add(new BattleLogEntry
                    {
                        Actor = user.Name,
                        Action = skill.Name,
                        Target = effectTarget.Name,
                        StatusEffect = skill.AppliesEffect.Value.ToString(),
                        Message = $"{user.Name}'s {skill.Name} applies {skill.AppliesEffect.Value} to {effectTarget.Name} for {skill.EffectDuration} turns."
                    });
                }

                if (skill.TargetType == SkillTargetType.Self && skill.BaseDamage == 0)
                {
                    result.Log.Add(new BattleLogEntry
                    {
                        Actor = user.Name,
                        Action = skill.Name,
                        Target = user.Name,
                        Message = $"{user.Name} uses {skill.Name}."
                    });
                }
            }

            // Check boss phase transitions for any damaged targets
            foreach (var t in targets.Where(t => t.Type == ParticipantType.Enemy))
            {
                await CheckBossPhaseTransition(battle, t, result);
            }
        }

        private async Task ProcessItemUse(Battle battle, BattleParticipant player, Guid inventoryItemId, BattleTurnResult result)
        {
            var invItem = await _db.InventoryItems
                .Include(ii => ii.Item)
                .FirstOrDefaultAsync(ii => ii.Id == inventoryItemId && ii.CharacterId == battle.CharacterId);

            if (invItem == null)
                throw new NotFoundException("Item not found in inventory.");

            if (invItem.Item.Type != ItemType.Consumable)
                throw new ValidationException("Can only use consumable items in battle.");

            if (invItem.Quantity <= 0)
                throw new ValidationException("No items remaining.");

            invItem.Quantity--;
            if (invItem.Quantity <= 0)
                _db.InventoryItems.Remove(invItem);

            var item = invItem.Item;
            var message = $"{player.Name} uses {item.Name}.";

            if (item.HealAmount > 0)
            {
                var healed = Math.Min(item.HealAmount, player.MaxHp - player.CurrentHp);
                player.CurrentHp += healed;
                message += $" Restored {healed} HP.";

                result.Log.Add(new BattleLogEntry
                {
                    Actor = player.Name,
                    Action = "UseItem",
                    Healing = healed,
                    Message = message
                });
            }

            if (item.ManaAmount > 0)
            {
                var restored = Math.Min(item.ManaAmount, player.MaxMp - player.CurrentMp);
                player.CurrentMp += restored;
                message = $"{player.Name} uses {item.Name}. Restored {restored} MP.";

                result.Log.Add(new BattleLogEntry
                {
                    Actor = player.Name,
                    Action = "UseItem",
                    Message = message
                });
            }

            if (item.BuffEffect.HasValue)
            {
                await _statusProcessor.ApplyEffectAsync(
                    battle.Id, player.Id,
                    item.BuffEffect.Value,
                    item.BuffDuration, 0, 1);

                result.Log.Add(new BattleLogEntry
                {
                    Actor = player.Name,
                    Action = "UseItem",
                    StatusEffect = item.BuffEffect.Value.ToString(),
                    Message = $"{player.Name} uses {item.Name}. Applied {item.BuffEffect.Value} for {item.BuffDuration} turns."
                });
            }
        }

        private async Task ProcessFlee(Battle battle, BattleParticipant player, BattleTurnResult result)
        {
            var rng = new Random();
            // Flee chance based on speed: base 30% + (player_speed / (player_speed + enemy_avg_speed)) * 40%
            var enemyAvgSpeed = battle.Participants
                .Where(p => p.Type == ParticipantType.Enemy && p.IsAlive)
                .Average(p => p.Speed);

            var fleeChance = 0.3 + (player.Speed / (player.Speed + enemyAvgSpeed)) * 0.4;

            if (rng.NextDouble() < fleeChance)
            {
                battle.Status = BattleStatus.Fled;
                battle.CompletedAt = DateTime.UtcNow;

                result.Log.Add(new BattleLogEntry
                {
                    Actor = player.Name,
                    Action = "Flee",
                    Message = "You successfully fled from battle!"
                });
                result.Status = BattleStatus.Fled;

                await SyncCharacterState(battle);
            }
            else
            {
                result.Log.Add(new BattleLogEntry
                {
                    Actor = player.Name,
                    Action = "Flee",
                    Message = "Failed to flee!"
                });
            }

            await _db.SaveChangesAsync();
        }

        private async Task ProcessEnemyAction(Battle battle, BattleParticipant enemy, BattleTurnResult result)
        {
            // Get enemy's skills
            var enemyEntity = await _db.Enemies
                .Include(e => e.Skills)
                .FirstOrDefaultAsync(e => e.Id == enemy.EnemyId);

            var skills = enemyEntity?.Skills.ToList() ?? new List<Skill>();
            var cooldowns = new List<LearnedSkill>(); // Enemies don't track cooldowns persistently in simplified model

            // Choose AI strategy based on whether it's a boss
            IEnemyAIStrategy ai = (enemyEntity?.IsBoss == true) ? new BossEnemyAI() : new BasicEnemyAI();
            var action = ai.DecideAction(enemy, battle.Participants.ToList(), skills, cooldowns);

            var players = battle.Participants.Where(p => p.Type == ParticipantType.Player && p.IsAlive).ToList();
            var target = action.TargetId.HasValue
                ? battle.Participants.FirstOrDefault(p => p.Id == action.TargetId.Value && p.IsAlive)
                : players.FirstOrDefault();

            switch (action.ActionType)
            {
                case BattleActionType.Attack:
                    if (target != null)
                        await ProcessBasicAttack(battle, enemy, target, result);
                    break;

                case BattleActionType.UseSkill when action.SkillId.HasValue:
                    var skill = skills.FirstOrDefault(s => s.Id == action.SkillId.Value);
                    if (skill != null && enemy.CurrentMp >= skill.MpCost)
                    {
                        enemy.CurrentMp -= skill.MpCost;

                        var targets = new List<BattleParticipant>();
                        if (skill.TargetType == SkillTargetType.Self)
                            targets.Add(enemy);
                        else if (skill.TargetType == SkillTargetType.All)
                            targets.AddRange(players);
                        else if (target != null)
                            targets.Add(target);

                        foreach (var t in targets)
                        {
                            if (skill.BaseDamage > 0 && skill.TargetType != SkillTargetType.Self)
                            {
                                var dmgResult = _damageCalc.CalculateSkillDamage(enemy, t, skill);
                                if (!dmgResult.IsDodged)
                                {
                                    var actual = await ApplyShieldAbsorption(battle.Id, t.Id, dmgResult.Damage);
                                    t.CurrentHp = Math.Max(t.CurrentHp - actual, 0);

                                    result.Log.Add(new BattleLogEntry
                                    {
                                        Actor = enemy.Name,
                                        Action = skill.Name,
                                        Target = t.Name,
                                        Damage = actual,
                                        IsCritical = dmgResult.IsCritical,
                                        Message = $"{enemy.Name} uses {skill.Name} on {t.Name} for {actual} damage.{(dmgResult.IsCritical ? " Critical hit!" : "")}"
                                    });
                                }
                                else
                                {
                                    result.Log.Add(new BattleLogEntry
                                    {
                                        Actor = enemy.Name,
                                        Action = skill.Name,
                                        Target = t.Name,
                                        IsDodged = true,
                                        Message = $"{t.Name} dodged {enemy.Name}'s {skill.Name}!"
                                    });
                                }
                            }

                            if (skill.AppliesEffect.HasValue)
                            {
                                var effectTarget = skill.TargetType == SkillTargetType.Self ? enemy : t;
                                await _statusProcessor.ApplyEffectAsync(
                                    battle.Id, effectTarget.Id,
                                    skill.AppliesEffect.Value,
                                    skill.EffectDuration,
                                    skill.EffectTickValue,
                                    skill.EffectStackLimit);

                                result.Log.Add(new BattleLogEntry
                                {
                                    Actor = enemy.Name,
                                    Action = skill.Name,
                                    StatusEffect = skill.AppliesEffect.Value.ToString(),
                                    Message = $"{enemy.Name}'s {skill.Name} applies {skill.AppliesEffect.Value} to {effectTarget.Name}."
                                });
                            }
                        }

                        if (skill.TargetType == SkillTargetType.Self && skill.BaseDamage == 0)
                        {
                            result.Log.Add(new BattleLogEntry
                            {
                                Actor = enemy.Name,
                                Action = skill.Name,
                                Message = $"{enemy.Name} uses {skill.Name}."
                            });
                        }
                    }
                    else if (target != null)
                    {
                        // Fallback to basic attack if skill can't be used
                        await ProcessBasicAttack(battle, enemy, target, result);
                    }
                    break;
            }
        }

        private async Task<int> ApplyShieldAbsorption(Guid battleId, Guid targetId, int rawDamage)
        {
            var shieldValue = await _statusProcessor.GetShieldValueAsync(battleId, targetId);
            if (shieldValue <= 0) return rawDamage;

            var absorbed = Math.Min(shieldValue, rawDamage);
            await _statusProcessor.ReduceShieldAsync(battleId, targetId, absorbed);
            return rawDamage - absorbed;
        }

        private async Task CheckBossPhaseTransition(Battle battle, BattleParticipant target, BattleTurnResult result)
        {
            if (target.Type != ParticipantType.Enemy || !target.EnemyId.HasValue || target.IsPhaseTwo)
                return;

            var enemy = await _db.Enemies.FindAsync(target.EnemyId.Value);
            if (enemy == null || !enemy.IsBoss) return;

            var hpPercent = (double)target.CurrentHp / target.MaxHp;
            if (hpPercent > 0.5) return;

            // Phase 2 transition!
            target.IsPhaseTwo = true;

            switch (enemy.BossMechanic)
            {
                case BossMechanic.Enrage:
                    target.Attack = (int)(target.Attack * 1.5);
                    target.Speed = (int)(target.Speed * 1.3);
                    result.Log.Add(new BattleLogEntry
                    {
                        Actor = target.Name,
                        Action = "Phase Transition",
                        Message = $"{target.Name} enters an enraged state! Attack and Speed increased!"
                    });
                    break;

                case BossMechanic.ShieldPhase:
                    await _statusProcessor.ApplyEffectAsync(
                        battle.Id, target.Id,
                        StatusEffectType.Shield, 3, target.MaxHp / 4, 1);
                    result.Log.Add(new BattleLogEntry
                    {
                        Actor = target.Name,
                        Action = "Phase Transition",
                        Message = $"{target.Name} erects a powerful shield!"
                    });
                    break;

                case BossMechanic.Summon:
                    result.Log.Add(new BattleLogEntry
                    {
                        Actor = target.Name,
                        Action = "Phase Transition",
                        Message = $"{target.Name} summons minions to aid in battle!"
                    });
                    // Add a weaker minion to the battle
                    var minion = new BattleParticipant
                    {
                        Id = Guid.NewGuid(),
                        BattleId = battle.Id,
                        Type = ParticipantType.Enemy,
                        EnemyId = target.EnemyId,
                        Name = $"{target.Name}'s Minion",
                        CurrentHp = target.MaxHp / 4,
                        MaxHp = target.MaxHp / 4,
                        CurrentMp = 0,
                        MaxMp = 0,
                        Attack = target.Attack / 2,
                        Defense = target.Defense / 2,
                        MagicPower = target.MagicPower / 2,
                        Speed = target.Speed,
                        CritChance = target.CritChance / 2,
                        DodgeChance = target.DodgeChance / 2,
                        TurnOrder = battle.Participants.Count
                    };
                    _db.BattleParticipants.Add(minion);
                    battle.Participants.Add(minion);
                    break;
            }
        }

        private BattleStatus? CheckBattleEnd(Battle battle)
        {
            if (battle.Status != BattleStatus.InProgress)
                return battle.Status;

            var playersAlive = battle.Participants.Any(p => p.Type == ParticipantType.Player && p.IsAlive);
            var enemiesAlive = battle.Participants.Any(p => p.Type == ParticipantType.Enemy && p.IsAlive);

            if (!enemiesAlive) return BattleStatus.Victory;
            if (!playersAlive) return BattleStatus.Defeat;

            return null;
        }

        private async Task HandleVictory(Battle battle, Character character, BattleTurnResult result)
        {
            // Calculate rewards from defeated enemies
            int totalXp = 0;
            int totalGold = 0;
            var allLoot = new List<DroppedItem>();

            foreach (var enemy in battle.Participants.Where(p => p.Type == ParticipantType.Enemy && p.EnemyId.HasValue))
            {
                var enemyDef = await _db.Enemies.FindAsync(enemy.EnemyId!.Value);
                if (enemyDef != null)
                {
                    totalXp += enemyDef.BaseXpReward;
                    totalGold += enemyDef.BaseGoldReward;

                    // Roll loot
                    var loot = await _lootService.RollLootAsync(enemyDef.Id);
                    allLoot.AddRange(loot);
                }
            }

            battle.XpReward = totalXp;
            battle.GoldReward = totalGold;

            // Award XP and gold to character
            character.Gold += totalGold;
            await _progression.AwardExperienceAsync(character.Id, totalXp);

            // Add loot to inventory
            if (allLoot.Any())
            {
                await _lootService.AddItemsToInventoryAsync(character.Id, allLoot);
            }

            result.XpAwarded = totalXp;
            result.GoldAwarded = totalGold;
            result.Loot = allLoot;

            result.Log.Add(new BattleLogEntry
            {
                Actor = "System",
                Action = "Victory",
                Message = $"Victory! Earned {totalXp} XP and {totalGold} gold.{(allLoot.Any() ? $" Received {allLoot.Count} item(s)." : "")}"
            });
        }

        /// <summary>
        /// Sync battle participant HP/MP back to the character entity after battle ends or turn completes.
        /// </summary>
        private async Task SyncCharacterState(Battle battle)
        {
            var playerParticipant = battle.Participants.FirstOrDefault(p => p.Type == ParticipantType.Player);
            if (playerParticipant?.CharacterId == null) return;

            var character = await _db.Characters.FindAsync(playerParticipant.CharacterId.Value);
            if (character == null) return;

            character.CurrentHp = Math.Max(playerParticipant.CurrentHp, 0);
            character.CurrentMp = Math.Max(playerParticipant.CurrentMp, 0);

            // Reset skill cooldowns after battle ends
            if (battle.Status != BattleStatus.InProgress)
            {
                var learnedSkills = await _db.LearnedSkills
                    .Where(ls => ls.CharacterId == character.Id)
                    .ToListAsync();
                foreach (var ls in learnedSkills)
                    ls.CurrentCooldown = 0;
            }
        }
    }
}
