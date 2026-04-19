using TestFirstProject.Models.Game;
using TestFirstProject.Models.Game.Enums;

namespace TestFirstProject.Services.Interfaces.Game
{
    /// <summary>
    /// Strategy pattern for enemy AI decision-making.
    /// Different enemy types can have different AI behaviors.
    /// </summary>
    public interface IEnemyAIStrategy
    {
        /// <summary>Determine the enemy's action for their turn.</summary>
        EnemyAction DecideAction(BattleParticipant enemy, List<BattleParticipant> allParticipants, List<Skill> availableSkills, List<LearnedSkill> cooldowns);
    }

    public class EnemyAction
    {
        public BattleActionType ActionType { get; set; }
        public Guid? SkillId { get; set; }
        public Guid? TargetId { get; set; }
    }
}
