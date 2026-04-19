using TestFirstProject.Models.Game;
using TestFirstProject.Models.Game.Enums;
using TestFirstProject.Services.Interfaces.Game;

namespace TestFirstProject.Services.Implementations.Game.EnemyAI
{
    /// <summary>
    /// Basic enemy AI: uses skills when available, falls back to basic attacks.
    /// Selects actions based on HP thresholds and skill cooldowns.
    /// </summary>
    public class BasicEnemyAI : IEnemyAIStrategy
    {
        private static readonly Random _rng = new();

        public EnemyAction DecideAction(BattleParticipant enemy, List<BattleParticipant> allParticipants, List<Skill> availableSkills, List<LearnedSkill> cooldowns)
        {
            var players = allParticipants.Where(p => p.Type == ParticipantType.Player && p.IsAlive).ToList();
            if (!players.Any())
                return new EnemyAction { ActionType = BattleActionType.Attack };

            // Pick a target (lowest HP player)
            var target = players.OrderBy(p => p.CurrentHp).First();

            // Try to use a skill if available and off cooldown
            var readySkills = availableSkills
                .Where(s => s.Type == SkillType.Active && s.MpCost <= enemy.CurrentMp)
                .Where(s =>
                {
                    var cd = cooldowns.FirstOrDefault(c => c.SkillId == s.Id);
                    return cd == null || cd.CurrentCooldown <= 0;
                })
                .ToList();

            if (readySkills.Any() && _rng.NextDouble() < 0.6) // 60% chance to use skill vs basic attack
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

            // Basic attack
            return new EnemyAction
            {
                ActionType = BattleActionType.Attack,
                TargetId = target.Id
            };
        }
    }
}
