using TestFirstProject.Models.Game;
using TestFirstProject.Models.Game.Enums;

namespace TestFirstProject.Services.Interfaces.Game
{
    /// <summary>
    /// Single processor for all status effect logic: application, tick processing, and removal.
    /// </summary>
    public interface IStatusEffectProcessor
    {
        /// <summary>Apply a status effect to a target participant. Handles stacking.</summary>
        Task<ActiveStatusEffect?> ApplyEffectAsync(Guid battleId, Guid targetParticipantId, StatusEffectType type, int duration, int tickValue, int stackLimit);

        /// <summary>Process all active effects on a participant at start of their turn. Returns log entries.</summary>
        Task<List<BattleLogEntry>> ProcessTurnStartEffectsAsync(Guid battleId, BattleParticipant participant);

        /// <summary>Check if a participant is stunned.</summary>
        Task<bool> IsStunnedAsync(Guid battleId, Guid participantId);

        /// <summary>Remove expired effects and decrement durations.</summary>
        Task TickEffectsAsync(Guid battleId, Guid participantId);

        /// <summary>Get active shield value for damage reduction.</summary>
        Task<int> GetShieldValueAsync(Guid battleId, Guid participantId);

        /// <summary>Reduce shield by absorbed damage.</summary>
        Task ReduceShieldAsync(Guid battleId, Guid participantId, int damageAbsorbed);
    }
}
