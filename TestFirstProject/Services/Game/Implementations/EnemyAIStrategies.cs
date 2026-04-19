using TestFirstProject.Models.Enums;
using TestFirstProject.Models.Game;
using TestFirstProject.Services.Game.Interfaces;

namespace TestFirstProject.Services.Game.Implementations
{
    /// <summary>
    /// Shared helpers for all AI strategies — targeting, skill filtering, random selection.
    /// </summary>
    internal static class AIHelpers
    {
        public static BattleParticipant? GetRandomAlivePlayer(Battle battle)
        {
            var alivePlayers = battle.Participants
                .Where(p => p.IsPlayer && p.IsAlive)
                .ToList();

            return alivePlayers.Count == 0
                ? null
                : alivePlayers[Random.Shared.Next(alivePlayers.Count)];
        }

        public static BattleParticipant? GetLowestHPPlayer(Battle battle)
        {
            return battle.Participants
                .Where(p => p.IsPlayer && p.IsAlive)
                .OrderBy(p => p.CurrentHP)
                .FirstOrDefault();
        }

        /// <summary>
        /// Base filter: active skills the enemy can afford to cast right now.
        /// </summary>
        public static IEnumerable<EnemySkill> GetUsableSkills(BattleParticipant enemy)
        {
            var skills = enemy.Enemy?.Skills;
            if (skills == null || skills.Count == 0)
                return Enumerable.Empty<EnemySkill>();

            return skills.Where(es => es.Skill != null
                                      && es.Skill.Type == SkillType.Active
                                      && es.Skill.ManaCost <= enemy.CurrentMP);
        }

        public static EnemySkill? GetHighestDamageSkill(BattleParticipant enemy)
        {
            return GetUsableSkills(enemy)
                .OrderByDescending(es => es.Skill.DamageMultiplier)
                .FirstOrDefault();
        }

        public static EnemySkill? GetRandomUsableSkill(BattleParticipant enemy)
        {
            var usable = GetUsableSkills(enemy).ToList();
            return usable.Count == 0
                ? null
                : usable[Random.Shared.Next(usable.Count)];
        }

        public static EnemySkill? GetDefensiveSkill(BattleParticipant enemy)
        {
            return GetUsableSkills(enemy)
                .Where(es => es.Skill.TargetType == TargetType.Self
                             || es.Skill.AppliesEffect == StatusEffectType.Shield
                             || es.Skill.AppliesEffect == StatusEffectType.Regen)
                .FirstOrDefault();
        }
    }

    /// <summary>
    /// Default AI: basic attacks with a 40% chance to use a skill when available.
    /// </summary>
    public class DefaultAIStrategy : IEnemyAIStrategy
    {
        public string StrategyName => "Default";

        public (BattleAction Action, Guid? SkillId, Guid? TargetId) SelectAction(
            BattleParticipant enemy, Battle battle)
        {
            var target = AIHelpers.GetRandomAlivePlayer(battle);
            if (target == null)
                return (BattleAction.Attack, null, null);

            var usableSkills = AIHelpers.GetUsableSkills(enemy).ToList();

            if (usableSkills.Count > 0 && Random.Shared.NextDouble() < 0.4)
            {
                var chosen = usableSkills[Random.Shared.Next(usableSkills.Count)];
                return (BattleAction.UseSkill, chosen.SkillId, target.Id);
            }

            return (BattleAction.Attack, null, target.Id);
        }
    }

    /// <summary>
    /// Aggressive AI: always prefers the highest-damage skill, targeting the player with the lowest HP.
    /// </summary>
    public class AggressiveAIStrategy : IEnemyAIStrategy
    {
        public string StrategyName => "Aggressive";

        public (BattleAction Action, Guid? SkillId, Guid? TargetId) SelectAction(
            BattleParticipant enemy, Battle battle)
        {
            var target = AIHelpers.GetLowestHPPlayer(battle);
            if (target == null)
                return (BattleAction.Attack, null, null);

            var bestSkill = AIHelpers.GetHighestDamageSkill(enemy);

            if (bestSkill != null)
                return (BattleAction.UseSkill, bestSkill.SkillId, target.Id);

            return (BattleAction.Attack, null, target.Id);
        }
    }

    /// <summary>
    /// Defensive AI: basic attacks when healthy, switches to healing/support skills below 50% HP.
    /// </summary>
    public class DefensiveAIStrategy : IEnemyAIStrategy
    {
        public string StrategyName => "Defensive";

        public (BattleAction Action, Guid? SkillId, Guid? TargetId) SelectAction(
            BattleParticipant enemy, Battle battle)
        {
            var target = AIHelpers.GetRandomAlivePlayer(battle);
            if (target == null)
                return (BattleAction.Attack, null, null);

            bool isLowHP = enemy.CurrentHP <= enemy.MaxHP / 2;

            if (isLowHP)
            {
                var healingSkill = AIHelpers.GetDefensiveSkill(enemy);
                if (healingSkill != null)
                {
                    var selfTarget = healingSkill.Skill.TargetType == TargetType.Self
                        ? enemy.Id
                        : target.Id;
                    return (BattleAction.UseSkill, healingSkill.SkillId, selfTarget);
                }
            }

            return (BattleAction.Attack, null, target.Id);
        }
    }

    /// <summary>
    /// Boss AI: two-phase behavior with mechanics that vary by BossMechanic type.
    /// Phase 1 (HP > 50%): cycles between attacks and skills.
    /// Phase 2 (HP &lt;= 50%): behavior depends on BossMechanic (Enrage, ShieldPhase, Summon).
    /// </summary>
    public class BossAIStrategy : IEnemyAIStrategy
    {
        public string StrategyName => "Boss";

        public (BattleAction Action, Guid? SkillId, Guid? TargetId) SelectAction(
            BattleParticipant enemy, Battle battle)
        {
            var target = AIHelpers.GetRandomAlivePlayer(battle);
            if (target == null)
                return (BattleAction.Attack, null, null);

            if (enemy.IsPhase2)
            {
                return SelectPhase2Action(enemy, battle, target);
            }

            return SelectPhase1Action(enemy, target);
        }

        private static (BattleAction Action, Guid? SkillId, Guid? TargetId) SelectPhase1Action(
            BattleParticipant enemy, BattleParticipant target)
        {
            if (Random.Shared.NextDouble() < 0.5)
            {
                var skill = AIHelpers.GetRandomUsableSkill(enemy);
                if (skill != null)
                    return (BattleAction.UseSkill, skill.SkillId, target.Id);
            }

            return (BattleAction.Attack, null, target.Id);
        }

        private static (BattleAction Action, Guid? SkillId, Guid? TargetId) SelectPhase2Action(
            BattleParticipant enemy, Battle battle, BattleParticipant target)
        {
            var mechanic = enemy.Enemy?.BossMechanic ?? BossMechanic.None;

            return mechanic switch
            {
                BossMechanic.Enrage => SelectEnrageAction(enemy, battle, target),
                BossMechanic.ShieldPhase => SelectShieldAction(enemy, target),
                BossMechanic.Summon => (BattleAction.Attack, null, target.Id),
                _ => SelectPhase1Action(enemy, target)
            };
        }

        private static (BattleAction Action, Guid? SkillId, Guid? TargetId) SelectEnrageAction(
            BattleParticipant enemy, Battle battle, BattleParticipant defaultTarget)
        {
            var target = AIHelpers.GetLowestHPPlayer(battle) ?? defaultTarget;

            if (Random.Shared.NextDouble() < 0.7)
            {
                var strongestSkill = AIHelpers.GetHighestDamageSkill(enemy);
                if (strongestSkill != null)
                    return (BattleAction.UseSkill, strongestSkill.SkillId, target.Id);
            }

            return (BattleAction.Attack, null, target.Id);
        }

        private static (BattleAction Action, Guid? SkillId, Guid? TargetId) SelectShieldAction(
            BattleParticipant enemy, BattleParticipant target)
        {
            var defensiveSkill = AIHelpers.GetDefensiveSkill(enemy);
            if (defensiveSkill != null)
            {
                var skillTarget = defensiveSkill.Skill.TargetType == TargetType.Self
                    ? enemy.Id
                    : target.Id;
                return (BattleAction.UseSkill, defensiveSkill.SkillId, skillTarget);
            }

            return (BattleAction.Attack, null, target.Id);
        }
    }
}
