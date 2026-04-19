using TestFirstProject.DTOs.Game;

namespace TestFirstProject.Services.Game.Interfaces
{
    /// <summary>
    /// Handles character creation, level-up, XP curves, and stat allocation.
    /// </summary>
    public interface ICharacterProgressionService
    {
        /// <summary>Create a new character for the authenticated player.</summary>
        Task<CharacterResponse> CreateCharacterAsync(Guid playerId, CreateCharacterRequest request);

        /// <summary>List all characters owned by the authenticated player.</summary>
        Task<List<CharacterResponse>> GetCharactersAsync(Guid playerId);

        /// <summary>Get the full stat sheet including equipment bonuses for a character.</summary>
        Task<CharacterStatsResponse> GetCharacterStatsAsync(Guid playerId, Guid characterId);

        /// <summary>Spend unallocated skill points to boost stats.</summary>
        Task<CharacterStatsResponse> AllocateStatsAsync(Guid playerId, Guid characterId, AllocateStatsRequest request);

        /// <summary>Award XP to a character and handle level-up if threshold is reached.</summary>
        Task AwardExperienceAsync(Guid characterId, int xp);

        /// <summary>Award gold to a character.</summary>
        Task AwardGoldAsync(Guid characterId, int gold);

        /// <summary>Calculate the XP required for a given level.</summary>
        int GetXPForLevel(int level);
    }
}
