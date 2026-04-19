using Microsoft.EntityFrameworkCore;
using TestFirstProject.Contexts;
using TestFirstProject.DTOs.Game;
using TestFirstProject.Exceptions;
using TestFirstProject.Models.Enums;
using TestFirstProject.Models.Game;
using TestFirstProject.Services.Game.Interfaces;

namespace TestFirstProject.Services.Game.Implementations
{
    /// <summary>
    /// Core turn-based combat engine. Manages battle lifecycle, turn order, actions, and AI.
    /// All combat state is persisted in DB — players can disconnect and resume.
    /// </summary>
    public class CombatEngine : ICombatEngine
    {
        private readonly PersonsContext _db;
        private readonly DamageCalculator _damageCalc;
        private readonly StatusEffectProcessor _statusProcessor;
        private readonly ICharacterProgressionService _progression;
        private readonly ILootService _lootService;
        private readonly IQuestProgressTracker _questTracker;
        private readonly IEnumerable<IEnemyAIStrategy> _aiStrategies;

        public CombatEngine(
            PersonsContext db,
            DamageCalculator damageCalc,
            StatusEffectProcessor statusProcessor,
            ICharacterProgressionService progression,
            ILootService lootService,
            IQuestProgressTracker questTracker,
            IEnumerable<IEnemyAIStrategy> aiStrategies)
        {
            _db = db;
            _damageCalc = damageCalc;
            _statusProcessor = statusProcessor;
            _progression = progression;
            _lootService = lootService;
            _questTracker = questTracker;
            _aiStrategies = aiStrategies;
        }

        public async Task<BattleStateResponse> StartBattleAsync(Guid playerId, StartBattleRequest request)
        {
            var character = await _db.Characters
                .Include(c => c.Stats)
                .Include(c => c.EquippedItems).ThenInclude(e => e.InventoryItem).ThenInclude(i => i.Item)
                .FirstOrDefaultAsync(c => c.Id == request.CharacterId && c.PlayerId == playerId)
                ?? throw new NotFoundException("Character not found.");

            Enemy enemy;
            if (request.EnemyId.HasValue)
            {
                enemy = await _db.Enemies.Include(e => e.Skills).ThenInclude(es => es.Skill)
                    .FirstOrDefaultAsync(e => e.Id == request.EnemyId.Value)
                    ?? throw new NotFoundException("Enemy not found.");
            }
            else if (request.ZoneId.HasValue)
            {
                var zoneEnemies = await _db.Enemies
                    .Include(e => e.Skills).ThenInclude(es => es.Skill)
                    .Where(e => e.ZoneId == request.ZoneId.Value && !e.IsBoss)
                    .ToListAsync();

                if (zoneEnemies.Count == 0)
                    throw new NotFoundException("No enemies found in this zone.");

                enemy = zoneEnemies[Random.Shared.Next(zoneEnemies.Count)];
            }
            else
            {
                throw new ValidationException("Either EnemyId or ZoneId must be provided.");
            }

            var activeBattle = await _db.Battles
                .AnyAsync(b => b.CharacterId == character.Id && b.Status == BattleStatus.InProgress);
            if (activeBattle)
                throw new ConflictException("Character is already in an active battle.");

            var battle = new Battle
            {
                Id = Guid.NewGuid(),
                CharacterId = character.Id,
                Status = BattleStatus.InProgress,
                CurrentTurn = 1,
                CreatedAt = DateTime.UtcNow
            };

            var equipBonuses = EquipmentBonusCalculator.Calculate(character.EquippedItems);
            var playerParticipant = new BattleParticipant
            {
                Id = Guid.NewGuid(),
                BattleId = battle.Id,
                CharacterId = character.Id,
                IsPlayer = true,
                Name = character.Name,
                CurrentHP = character.Stats.HP + equipBonuses.HP,
                MaxHP = character.Stats.MaxHP + equipBonuses.HP,
                CurrentMP = character.Stats.MP + equipBonuses.MP,
                MaxMP = character.Stats.MaxMP + equipBonuses.MP,
                Attack = character.Stats.Attack + equipBonuses.Attack,
                Defense = character.Stats.Defense + equipBonuses.Defense,
                MagicPower = character.Stats.MagicPower + equipBonuses.MagicPower,
                Speed = character.Stats.Speed + equipBonuses.Speed,
                CritChance = character.Stats.CritChance + equipBonuses.CritChance,
                DodgeChance = character.Stats.DodgeChance + equipBonuses.DodgeChance,
                IsAlive = true
            };

            var enemyParticipant = new BattleParticipant
            {
                Id = Guid.NewGuid(),
                BattleId = battle.Id,
                EnemyId = enemy.Id,
                IsPlayer = false,
                Name = enemy.Name,
                CurrentHP = enemy.BaseHP,
                MaxHP = enemy.BaseHP,
                CurrentMP = enemy.BaseMP,
                MaxMP = enemy.BaseMP,
                Attack = enemy.BaseAttack,
                Defense = enemy.BaseDefense,
                MagicPower = enemy.BaseMagicPower,
                Speed = enemy.BaseSpeed,
                CritChance = enemy.BaseCritChance,
                DodgeChance = enemy.BaseDodgeChance,
                IsAlive = true,
                IsPhase2 = false
            };

            battle.CurrentTurnParticipantId = playerParticipant.Speed >= enemyParticipant.Speed
                ? playerParticipant.Id
                : enemyParticipant.Id;

            battle.Participants.Add(playerParticipant);
            battle.Participants.Add(enemyParticipant);
            _db.Battles.Add(battle);

            await _db.SaveChangesAsync();

            return BuildBattleStateResponse(battle);
        }

        public async Task<BattleStateResponse> GetBattleStateAsync(Guid playerId, Guid battleId)
        {
            var battle = await LoadBattleWithFullGraph(battleId);

            var character = await _db.Characters
                .FirstOrDefaultAsync(c => c.Id == battle.CharacterId && c.PlayerId == playerId)
                ?? throw new ForbiddenException("You do not have access to this battle.");

            return BuildBattleStateResponse(battle);
        }

        public async Task<BattleStateResponse> SubmitActionAsync(Guid playerId, Guid battleId, BattleActionRequest request)
        {
            var battle = await LoadBattleWithFullGraph(battleId);

            if (battle.Status != BattleStatus.InProgress)
                throw new ValidationException($"Battle is already {battle.Status}.");

            var character = await _db.Characters
                .FirstOrDefaultAsync(c => c.Id == battle.CharacterId && c.PlayerId == playerId)
                ?? throw new ForbiddenException("You do not have access to this battle.");

            var currentParticipant = battle.Participants.FirstOrDefault(p => p.Id == battle.CurrentTurnParticipantId);
            if (currentParticipant == null || !currentParticipant.IsPlayer)
                throw new ValidationException("It is not your turn.");

            if (!Enum.TryParse<BattleAction>(request.Action, true, out var action))
                throw new ValidationException($"Invalid action: {request.Action}. Valid: Attack, UseSkill, UseItem, Flee");

            // Process status effects (returns stun/speed info — no extra DB queries needed)
            var effectResult = await _statusProcessor.ProcessTurnEffectsAsync(currentParticipant);
            foreach (var desc in effectResult.Descriptions)
            {
                AddTurnLog(battle, currentParticipant.Id, null, BattleAction.Attack, desc, actionDetail: "StatusEffect");
            }

            if (effectResult.IsStunned)
            {
                AddTurnLog(battle, currentParticipant.Id, null, BattleAction.Attack,
                    $"{currentParticipant.Name} is stunned and loses their turn!");
                await AdvanceTurn(battle);
                await _db.SaveChangesAsync();
                return BuildBattleStateResponse(battle);
            }

            if (!currentParticipant.IsAlive)
            {
                battle.Status = BattleStatus.Defeat;
                battle.CompletedAt = DateTime.UtcNow;
                await _db.SaveChangesAsync();
                return BuildBattleStateResponse(battle);
            }

            await ExecutePlayerAction(battle, currentParticipant, action, request);

            if (await CheckBattleEnd(battle))
            {
                await _db.SaveChangesAsync();
                return BuildBattleStateResponse(battle);
            }

            await AdvanceTurn(battle);
            await ProcessEnemyTurns(battle);

            await _db.SaveChangesAsync();
            return BuildBattleStateResponse(battle);
        }

        #region Private - Player Actions

        private async Task ExecutePlayerAction(Battle battle, BattleParticipant player, BattleAction action,
            BattleActionRequest request)
        {
            var enemies = battle.Participants.Where(p => !p.IsPlayer && p.IsAlive).ToList();

            switch (action)
            {
                case BattleAction.Attack:
                    var target = request.TargetId.HasValue
                        ? enemies.FirstOrDefault(e => e.Id == request.TargetId.Value)
                          ?? enemies.First()
                        : enemies.First();

                    var result = _damageCalc.CalculateBasicAttack(player, target);
                    ApplyDamage(target, result, battle, player);
                    break;

                case BattleAction.UseSkill:
                    if (!request.SkillId.HasValue)
                        throw new ValidationException("SkillId is required for UseSkill action.");

                    await ExecuteSkillAction(battle, player, request.SkillId.Value, request.TargetId, enemies);
                    break;

                case BattleAction.UseItem:
                    if (!request.ItemId.HasValue)
                        throw new ValidationException("ItemId is required for UseItem action.");

                    await ExecuteItemAction(battle, player, request.ItemId.Value);
                    break;

                case BattleAction.Flee:
                    var avgEnemySpeed = enemies.Average(e => e.Speed);
                    var fleeChance = 30 + ((player.Speed - avgEnemySpeed) * 2);
                    if (Random.Shared.NextDouble() * 100 < fleeChance)
                    {
                        battle.Status = BattleStatus.Fled;
                        battle.CompletedAt = DateTime.UtcNow;
                        AddTurnLog(battle, player.Id, null, BattleAction.Flee,
                            $"{player.Name} successfully fled from battle!");
                    }
                    else
                    {
                        AddTurnLog(battle, player.Id, null, BattleAction.Flee,
                            $"{player.Name} tried to flee but failed!");
                    }
                    break;
            }
        }

        private async Task ExecuteSkillAction(Battle battle, BattleParticipant caster, Guid skillId,
            Guid? targetId, List<BattleParticipant> enemies)
        {
            var learnedSkill = await _db.LearnedSkills
                .Include(ls => ls.Skill)
                .FirstOrDefaultAsync(ls => ls.SkillId == skillId && ls.CharacterId == caster.CharacterId)
                ?? throw new ValidationException("Character has not learned this skill.");

            var skill = learnedSkill.Skill;
            if (skill.Type != SkillType.Active)
                throw new ValidationException("Cannot use a passive skill in combat.");

            if (caster.CurrentMP < skill.ManaCost)
                throw new ValidationException($"Not enough MP. Need {skill.ManaCost}, have {caster.CurrentMP}.");

            caster.CurrentMP -= skill.ManaCost;

            switch (skill.TargetType)
            {
                case TargetType.Single:
                    var singleTarget = targetId.HasValue
                        ? enemies.FirstOrDefault(e => e.Id == targetId.Value) ?? enemies.First()
                        : enemies.First();

                    var dmg = _damageCalc.CalculateSkillDamage(caster, singleTarget, skill);
                    ApplyDamage(singleTarget, dmg, battle, caster, skill.Name);

                    if (skill.AppliesEffect.HasValue && !dmg.IsDodged)
                    {
                        await _statusProcessor.ApplyEffectAsync(singleTarget.Id, skill.AppliesEffect.Value,
                            skill.EffectDuration, skill.EffectTickValue);
                        AddTurnLog(battle, caster.Id, singleTarget.Id, BattleAction.UseSkill,
                            $"{skill.AppliesEffect.Value} applied to {singleTarget.Name} for {skill.EffectDuration} turns.",
                            actionDetail: skill.Name, statusEffect: skill.AppliesEffect.Value.ToString());
                    }
                    break;

                case TargetType.All:
                    foreach (var enemy in enemies)
                    {
                        var aoe = _damageCalc.CalculateSkillDamage(caster, enemy, skill);
                        ApplyDamage(enemy, aoe, battle, caster, skill.Name);

                        if (skill.AppliesEffect.HasValue && !aoe.IsDodged)
                        {
                            await _statusProcessor.ApplyEffectAsync(enemy.Id, skill.AppliesEffect.Value,
                                skill.EffectDuration, skill.EffectTickValue);
                        }
                    }
                    break;

                case TargetType.Self:
                    if (skill.AppliesEffect.HasValue)
                    {
                        await _statusProcessor.ApplyEffectAsync(caster.Id, skill.AppliesEffect.Value,
                            skill.EffectDuration, skill.EffectTickValue);
                        AddTurnLog(battle, caster.Id, caster.Id, BattleAction.UseSkill,
                            $"{caster.Name} uses {skill.Name}! {skill.AppliesEffect.Value} applied for {skill.EffectDuration} turns.",
                            actionDetail: skill.Name, statusEffect: skill.AppliesEffect.Value.ToString());
                    }
                    break;
            }
        }

        private async Task ExecuteItemAction(Battle battle, BattleParticipant player, Guid inventoryItemId)
        {
            var invItem = await _db.InventoryItems
                .Include(i => i.Item)
                .FirstOrDefaultAsync(i => i.Id == inventoryItemId && i.CharacterId == player.CharacterId)
                ?? throw new NotFoundException("Item not found in inventory.");

            if (invItem.Item.Type != ItemType.Consumable)
                throw new ValidationException("Only consumable items can be used in battle.");

            if (invItem.Quantity <= 0)
                throw new ValidationException("No items remaining.");

            switch (invItem.Item.ConsumableType)
            {
                case ConsumableType.HealthPotion:
                    var healAmt = _damageCalc.CalculateHealing(invItem.Item.ConsumableValue);
                    player.CurrentHP = Math.Min(player.MaxHP, player.CurrentHP + healAmt);
                    AddTurnLog(battle, player.Id, player.Id, BattleAction.UseItem,
                        $"{player.Name} uses {invItem.Item.Name} and restores {healAmt} HP! (HP: {player.CurrentHP}/{player.MaxHP})",
                        healingDone: healAmt, actionDetail: invItem.Item.Name);
                    break;

                case ConsumableType.ManaPotion:
                    var manaAmt = _damageCalc.CalculateHealing(invItem.Item.ConsumableValue);
                    player.CurrentMP = Math.Min(player.MaxMP, player.CurrentMP + manaAmt);
                    AddTurnLog(battle, player.Id, player.Id, BattleAction.UseItem,
                        $"{player.Name} uses {invItem.Item.Name} and restores {manaAmt} MP! (MP: {player.CurrentMP}/{player.MaxMP})",
                        healingDone: manaAmt, actionDetail: invItem.Item.Name);
                    break;

                case ConsumableType.Scroll:
                    if (invItem.Item.ScrollEffect.HasValue)
                    {
                        await _statusProcessor.ApplyEffectAsync(player.Id, invItem.Item.ScrollEffect.Value,
                            invItem.Item.ScrollEffectDuration, invItem.Item.ConsumableValue);
                        AddTurnLog(battle, player.Id, player.Id, BattleAction.UseItem,
                            $"{player.Name} uses {invItem.Item.Name}! {invItem.Item.ScrollEffect.Value} buff applied for {invItem.Item.ScrollEffectDuration} turns.",
                            actionDetail: invItem.Item.Name, statusEffect: invItem.Item.ScrollEffect.Value.ToString());
                    }
                    break;
            }

            invItem.Quantity--;
            if (invItem.Quantity <= 0)
                _db.InventoryItems.Remove(invItem);
        }

        #endregion

        #region Private - Enemy AI

        private async Task ProcessEnemyTurns(Battle battle)
        {
            while (battle.Status == BattleStatus.InProgress)
            {
                var current = battle.Participants.FirstOrDefault(p => p.Id == battle.CurrentTurnParticipantId);
                if (current == null || current.IsPlayer) break;
                if (!current.IsAlive)
                {
                    await AdvanceTurn(battle);
                    continue;
                }

                var effectResult = await _statusProcessor.ProcessTurnEffectsAsync(current);
                foreach (var desc in effectResult.Descriptions)
                {
                    AddTurnLog(battle, current.Id, null, BattleAction.Attack, desc, actionDetail: "StatusEffect");
                }

                if (!current.IsAlive || effectResult.IsStunned)
                {
                    if (await CheckBattleEnd(battle)) break;
                    await AdvanceTurn(battle);
                    continue;
                }

                // Boss phase transition (single authoritative location)
                if (current.Enemy?.IsBoss == true && !current.IsPhase2 &&
                    current.CurrentHP <= current.MaxHP / 2)
                {
                    current.IsPhase2 = true;
                    AddTurnLog(battle, current.Id, null, BattleAction.Attack,
                        $"{current.Name} enters Phase 2! The boss grows more powerful!",
                        actionDetail: "PhaseTransition");
                }

                var strategyName = current.Enemy?.AIStrategy ?? "Default";
                if (current.Enemy?.IsBoss == true) strategyName = "Boss";
                var strategy = _aiStrategies.FirstOrDefault(s => s.StrategyName == strategyName)
                               ?? _aiStrategies.First(s => s.StrategyName == "Default");

                var (aiAction, aiSkillId, aiTargetId) = strategy.SelectAction(current, battle);

                var playerTargets = battle.Participants.Where(p => p.IsPlayer && p.IsAlive).ToList();
                if (playerTargets.Count == 0) break;

                var targetParticipant = aiTargetId.HasValue
                    ? playerTargets.FirstOrDefault(p => p.Id == aiTargetId.Value) ?? playerTargets.First()
                    : playerTargets.First();

                switch (aiAction)
                {
                    case BattleAction.Attack:
                        var dmgResult = _damageCalc.CalculateBasicAttack(current, targetParticipant);
                        ApplyDamage(targetParticipant, dmgResult, battle, current);
                        break;

                    case BattleAction.UseSkill when aiSkillId.HasValue:
                        // Use the already-loaded skill from the Include chain instead of FindAsync
                        var skill = current.Enemy?.Skills
                            .FirstOrDefault(es => es.SkillId == aiSkillId.Value)?.Skill;

                        if (skill != null && current.CurrentMP >= skill.ManaCost)
                        {
                            current.CurrentMP -= skill.ManaCost;
                            if (skill.TargetType == TargetType.Self)
                            {
                                if (skill.AppliesEffect.HasValue)
                                {
                                    await _statusProcessor.ApplyEffectAsync(current.Id, skill.AppliesEffect.Value,
                                        skill.EffectDuration, skill.EffectTickValue);
                                    AddTurnLog(battle, current.Id, current.Id, BattleAction.UseSkill,
                                        $"{current.Name} uses {skill.Name}!",
                                        actionDetail: skill.Name, statusEffect: skill.AppliesEffect?.ToString());
                                }
                            }
                            else if (skill.TargetType == TargetType.All)
                            {
                                foreach (var pt in playerTargets)
                                {
                                    var aoe = _damageCalc.CalculateSkillDamage(current, pt, skill);
                                    ApplyDamage(pt, aoe, battle, current, skill.Name);
                                    if (skill.AppliesEffect.HasValue && !aoe.IsDodged)
                                    {
                                        await _statusProcessor.ApplyEffectAsync(pt.Id, skill.AppliesEffect.Value,
                                            skill.EffectDuration, skill.EffectTickValue);
                                    }
                                }
                            }
                            else
                            {
                                var skillDmg = _damageCalc.CalculateSkillDamage(current, targetParticipant, skill);
                                ApplyDamage(targetParticipant, skillDmg, battle, current, skill.Name);
                                if (skill.AppliesEffect.HasValue && !skillDmg.IsDodged)
                                {
                                    await _statusProcessor.ApplyEffectAsync(targetParticipant.Id,
                                        skill.AppliesEffect.Value, skill.EffectDuration, skill.EffectTickValue);
                                }
                            }
                        }
                        else
                        {
                            var fallback = _damageCalc.CalculateBasicAttack(current, targetParticipant);
                            ApplyDamage(targetParticipant, fallback, battle, current);
                        }
                        break;

                    default:
                        var basicDmg = _damageCalc.CalculateBasicAttack(current, targetParticipant);
                        ApplyDamage(targetParticipant, basicDmg, battle, current);
                        break;
                }

                if (await CheckBattleEnd(battle)) break;
                await AdvanceTurn(battle);
            }
        }

        #endregion

        #region Private - Battle State Management

        private void ApplyDamage(BattleParticipant target, DamageResult result, Battle battle,
            BattleParticipant attacker, string? skillName = null)
        {
            if (result.IsDodged)
            {
                AddTurnLog(battle, attacker.Id, target.Id,
                    skillName != null ? BattleAction.UseSkill : BattleAction.Attack,
                    $"{target.Name} dodges {attacker.Name}'s {skillName ?? "attack"}!",
                    wasDodged: true, actionDetail: skillName);
                return;
            }

            target.CurrentHP = Math.Max(0, target.CurrentHP - result.Damage);
            if (target.CurrentHP <= 0) target.IsAlive = false;

            var critText = result.IsCritical ? " CRITICAL HIT!" : "";
            var desc = skillName != null
                ? $"{attacker.Name} uses {skillName} on {target.Name} for {result.Damage} damage!{critText} (HP: {target.CurrentHP}/{target.MaxHP})"
                : $"{attacker.Name} attacks {target.Name} for {result.Damage} damage!{critText} (HP: {target.CurrentHP}/{target.MaxHP})";

            if (!target.IsAlive)
                desc += $" {target.Name} has been defeated!";

            AddTurnLog(battle, attacker.Id, target.Id,
                skillName != null ? BattleAction.UseSkill : BattleAction.Attack,
                desc, damageDealt: result.Damage, wasCritical: result.IsCritical,
                actionDetail: skillName);
        }

        private async Task<bool> CheckBattleEnd(Battle battle)
        {
            var allEnemiesDead = battle.Participants
                .Where(p => !p.IsPlayer)
                .All(p => !p.IsAlive);

            var allPlayersDead = battle.Participants
                .Where(p => p.IsPlayer)
                .All(p => !p.IsAlive);

            if (allEnemiesDead)
            {
                battle.Status = BattleStatus.Victory;
                battle.CompletedAt = DateTime.UtcNow;

                // Award XP, gold, loot, and track quests — all use the same DbContext,
                // so a single SaveChangesAsync at the end of SubmitActionAsync flushes everything.
                foreach (var enemyParticipant in battle.Participants.Where(p => !p.IsPlayer))
                {
                    // Use the already-loaded Enemy navigation instead of FindAsync
                    var enemyData = enemyParticipant.Enemy;
                    if (enemyData == null) continue;

                    await _progression.AwardExperienceAsync(battle.CharacterId, enemyData.ExperienceReward);
                    await _progression.AwardGoldAsync(battle.CharacterId, enemyData.GoldReward);
                    await _lootService.RollLootAsync(enemyData.Id, battle.CharacterId);
                    await _questTracker.TrackEnemyKillAsync(battle.CharacterId, enemyData.Name);
                    if (enemyData.IsBoss)
                        await _questTracker.TrackBossDefeatAsync(battle.CharacterId, enemyData.Name);
                }

                return true;
            }

            if (allPlayersDead)
            {
                battle.Status = BattleStatus.Defeat;
                battle.CompletedAt = DateTime.UtcNow;
                return true;
            }

            return false;
        }

        private async Task AdvanceTurn(Battle battle)
        {
            var aliveParticipants = battle.Participants.Where(p => p.IsAlive).ToList();
            if (aliveParticipants.Count == 0) return;

            // Batch-load all status effects for speed modifiers in a single query
            var participantIds = aliveParticipants.Select(p => p.Id).ToList();
            var speedEffects = await _db.Set<ActiveStatusEffect>()
                .Where(e => participantIds.Contains(e.BattleParticipantId) &&
                           (e.Type == StatusEffectType.Haste || e.Type == StatusEffectType.Slow))
                .ToListAsync();

            var speedList = aliveParticipants.Select(p =>
            {
                var modifier = 0;
                foreach (var effect in speedEffects.Where(e => e.BattleParticipantId == p.Id))
                {
                    modifier += effect.Type == StatusEffectType.Haste
                        ? (int)(p.Speed * 0.3)
                        : -(int)(p.Speed * 0.3);
                }
                return (Participant: p, EffectiveSpeed: p.Speed + modifier);
            })
            .OrderByDescending(s => s.EffectiveSpeed)
            .ToList();

            var currentIdx = speedList.FindIndex(s => s.Participant.Id == battle.CurrentTurnParticipantId);
            var nextIdx = (currentIdx + 1) % speedList.Count;

            if (nextIdx <= currentIdx)
                battle.CurrentTurn++;

            battle.CurrentTurnParticipantId = speedList[nextIdx].Participant.Id;
        }

        private void AddTurnLog(Battle battle, Guid actorId, Guid? targetId, BattleAction action,
            string description, int damageDealt = 0, int healingDone = 0, bool wasCritical = false,
            bool wasDodged = false, string? actionDetail = null, string? statusEffect = null)
        {
            battle.TurnLogs.Add(new BattleTurnLog
            {
                Id = Guid.NewGuid(),
                BattleId = battle.Id,
                TurnNumber = battle.CurrentTurn,
                ActorId = actorId,
                TargetId = targetId,
                Action = action,
                ActionDetail = actionDetail,
                DamageDealt = damageDealt,
                HealingDone = healingDone,
                WasCritical = wasCritical,
                WasDodged = wasDodged,
                StatusEffectApplied = statusEffect,
                Description = description,
                Timestamp = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Load a battle with full graph for read operations (GetBattleState, SubmitAction).
        /// </summary>
        private async Task<Battle> LoadBattleWithFullGraph(Guid battleId)
        {
            return await _db.Battles
                .Include(b => b.Participants).ThenInclude(p => p.StatusEffects)
                .Include(b => b.Participants).ThenInclude(p => p.Enemy).ThenInclude(e => e!.Skills).ThenInclude(es => es.Skill)
                .Include(b => b.TurnLogs)
                .FirstOrDefaultAsync(b => b.Id == battleId)
                ?? throw new NotFoundException("Battle not found.");
        }

        /// <summary>
        /// Build response from the in-memory battle entity (no re-query).
        /// </summary>
        private static BattleStateResponse BuildBattleStateResponse(Battle battle)
        {
            var participants = battle.Participants.Select(p => new BattleParticipantResponse(
                p.Id, p.Name, p.IsPlayer, p.CurrentHP, p.MaxHP, p.CurrentMP, p.MaxMP, p.IsAlive,
                p.StatusEffects.Select(e => new StatusEffectResponse(
                    e.Type.ToString(), e.RemainingTurns, e.TickValue, e.StackCount
                )).ToList()
            )).ToList();

            var recentLogs = battle.TurnLogs
                .OrderByDescending(t => t.Timestamp)
                .Take(10)
                .OrderBy(t => t.Timestamp)
                .Select(t =>
                {
                    var actorName = battle.Participants.FirstOrDefault(p => p.Id == t.ActorId)?.Name ?? "Unknown";
                    var targetName = t.TargetId.HasValue
                        ? battle.Participants.FirstOrDefault(p => p.Id == t.TargetId.Value)?.Name
                        : null;
                    return new BattleTurnLogResponse(
                        t.TurnNumber, actorName, targetName, t.Action.ToString(),
                        t.ActionDetail, t.DamageDealt, t.HealingDone,
                        t.WasCritical, t.WasDodged, t.Description
                    );
                }).ToList();

            return new BattleStateResponse(
                battle.Id, battle.Status.ToString(), battle.CurrentTurn,
                battle.CurrentTurnParticipantId, participants, recentLogs
            );
        }

        #endregion
    }
}
