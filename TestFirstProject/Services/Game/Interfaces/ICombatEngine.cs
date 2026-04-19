using TestFirstProject.DTOs.Game;

namespace TestFirstProject.Services.Game.Interfaces
{
    /// <summary>
    /// Core combat engine: start battles, get state, process turns.
    /// All combat state is persisted in DB.
    /// </summary>
    public interface ICombatEngine
    {
        /// <summary>Start a new battle for a character against enemies.</summary>
        Task<BattleStateResponse> StartBattleAsync(Guid playerId, StartBattleRequest request);

        /// <summary>Get the current state of an in-progress battle.</summary>
        Task<BattleStateResponse> GetBattleStateAsync(Guid playerId, Guid battleId);

        /// <summary>Submit a turn action for the current participant.</summary>
        Task<BattleStateResponse> SubmitActionAsync(Guid playerId, Guid battleId, BattleActionRequest request);
    }
}
