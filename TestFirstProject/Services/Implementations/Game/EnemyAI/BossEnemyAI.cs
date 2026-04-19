using TestFirstProject.Models.Game;
using TestFirstProject.Models.Game.Enums;
using TestFirstProject.Services.Interfaces.Game;

namespace TestFirstProject.Services.Implementations.Game.EnemyAI
{
    /// <summary>
    /// Boss enemy AI: more strategic, uses skills more often.
    /// In phase 2 (below 50% HP), behavior changes based on boss mechanic.
    /// </summary>
    public class BossEnemyAI : IEnemyAIStrategy
    {
        private static readonly Random _rng = new();

        public EnemyAction DecideAction(BattleParticipant enemy, List<BattleParticipant> allParticipants, List<Skill> availableSkills, List<LearnedSkill> cooldowns)
        {
            var players = allParticipants.Where(p => p.Type == ParticipantType.Player && p.IsAlive).ToList();
            if (!players.Any())
                return new EnemyAction { ActionType = BattleActionType.Attack };

            var target = players.OrderBy(p => p.CurrentHp).First();
            var hpPercent = (double)enemy.CurrentHp / enemy.MaxHp;

            // Ready skills
            var readySkills = availableSkills
                .Where(s => s.Type == SkillType.Active && s.MpCost <= enemy.CurrentMp)
                .Where(s =>
                {
                    var cd = cooldowns.FirstOrDefault(c => c.SkillId == s.Id);
                    return cd == null || cd.CurrentCooldown <= 0;
                })
                .ToList();

            // Phase 2: more aggressive, always tries to use skills
            if (hpPercent < 0.5)
            {
                // Prefer AoE skills in phase 2
                var aoeSkills = readySkills.Where(s => s.TargetType == SkillTargetType.All).ToList();
                if (aoeSkills.Any() && _rng.NextDouble() < 0.7)
                {
                    var skill = aoeSkills[_rng.Next(aoeSkills.Count)];
                    return new EnemyAction
                    {
                        ActionType = BattleActionType.UseSkill,
                        SkillId = skill.Id,
                        TargetId = target.Id
                    };
                }

                // Try self-buff skills (shield, haste)
                var selfSkills = readySkills.Where(s => s.TargetType == SkillTargetType.Self).ToList();
                if (selfSkills.Any() && _rng.NextDouble() < 0.4)
                {
                    var skill = selfSkills[_rng.Next(selfSkills.Count)];
                    return new EnemyAction
                    {
                        ActionType = BattleActionType.UseSkill,
                        SkillId = skill.Id,
                        TargetId = enemy.Id
                    };
                }
            }

            // Standard behavior: 80% chance to use skills for bosses
            if (readySkills.Any() && _rng.NextDouble() < 0.8)
            {
                var skill = readySkills[_rng.Next(readySkills.Count)];
                var skillTarget = skill.TargetType == SkillTargetType.Self ? enemy.Id : target.Id;

                return new EnemyAction
                {
                    ActionType = BattleActionType.UseSkill,
                    SkillId = skill.Id,
                    TargetId = skillTarget
                };
            }

            return new EnemyAction
            {
                ActionType = BattleActionType.Attack,
                TargetId = target.Id
            };
        }
    }
}
