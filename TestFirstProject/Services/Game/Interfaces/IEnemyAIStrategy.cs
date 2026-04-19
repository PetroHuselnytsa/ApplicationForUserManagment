using TestFirstProject.Models.Enums;
using TestFirstProject.Models.Game;

namespace TestFirstProject.Services.Game.Interfaces
{
    /// <summary>
    /// Strategy interface for enemy AI action selection.
    /// Different enemy types use different strategies.
    /// </summary>
    public interface IEnemyAIStrategy
    {
        /// <summary>The strategy name this implementation handles.</summary>
        string StrategyName { get; }

        /// <summary>
        /// Select the next action for an enemy participant based on battle state.
        /// </summary>
        /// <param name="enemy">The enemy participant.</param>
        /// <param name="battle">The current battle state.</param>
        /// <returns>A tuple of (action, skillId, targetId).</returns>
        (BattleAction Action, Guid? SkillId, Guid? TargetId) SelectAction(
            BattleParticipant enemy,
            Battle battle);
    }
}
