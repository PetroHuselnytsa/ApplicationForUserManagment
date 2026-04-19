using TestFirstProject.Models.Game;
using TestFirstProject.Models.Game.Enums;

namespace TestFirstProject.Services.Interfaces.Game
{
    /// <summary>
    /// Handles character creation, leveling, stat allocation, and skill unlocks.
    /// </summary>
    public interface ICharacterProgressionService
    {
        /// <summary>Create a new character for a player.</summary>
        Task<Character> CreateCharacterAsync(Guid playerId, string name, CharacterClass characterClass);

        /// <summary>Get all characters owned by a player.</summary>
        Task<List<Character>> GetPlayerCharactersAsync(Guid playerId);

        /// <summary>Get a character by ID with full stats and equipment.</summary>
        Task<Character> GetCharacterAsync(Guid characterId, Guid playerId);

        /// <summary>Award XP to a character, handling level-ups and skill unlocks.</summary>
        Task<Character> AwardExperienceAsync(Guid characterId, int xp);

        /// <summary>Allocate skill points to stats.</summary>
        Task<CharacterStats> AllocateStatsAsync(Guid characterId, Guid playerId, int hp, int mp, int attack, int defense, int magicPower, int speed);

        /// <summary>Get base stats for a class at a given level.</summary>
        CharacterStats CalculateBaseStats(CharacterClass characterClass, int level);

        /// <summary>Calculate the XP required to reach a given level.</summary>
        int GetXpForLevel(int level);

        /// <summary>Calculate total stats including equipment bonuses.</summary>
        Task<CharacterStats> GetTotalStatsAsync(Guid characterId);
    }
}
