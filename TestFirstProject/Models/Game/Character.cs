using TestFirstProject.Models.Game.Enums;

namespace TestFirstProject.Models.Game
{
    /// <summary>
    /// A playable character owned by a player (AppUser). Each player can own multiple characters.
    /// </summary>
    public class Character
    {
        public Guid Id { get; set; }
        public Guid PlayerId { get; set; }
        public AppUser Player { get; set; } = null!;

        public string Name { get; set; } = null!;
        public CharacterClass Class { get; set; }
        public int Level { get; set; } = 1;
        public int Experience { get; set; } = 0;
        public int SkillPoints { get; set; } = 0;

        // Current HP/MP (can change in combat)
        public int CurrentHp { get; set; }
        public int CurrentMp { get; set; }

        // Gold currency
        public int Gold { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public CharacterStats Stats { get; set; } = null!;
        public ICollection<LearnedSkill> LearnedSkills { get; set; } = new List<LearnedSkill>();
        public ICollection<InventoryItem> Inventory { get; set; } = new List<InventoryItem>();
        public ICollection<EquippedItem> EquippedItems { get; set; } = new List<EquippedItem>();
        public ICollection<CharacterQuest> Quests { get; set; } = new List<CharacterQuest>();
    }
}
