using TestFirstProject.Models.Enums;

namespace TestFirstProject.Models.Game
{
    /// <summary>
    /// A playable character owned by an AppUser.
    /// Each player can own multiple characters of different classes.
    /// </summary>
    public class Character
    {
        public Guid Id { get; set; }

        /// <summary>FK to the owning player (AppUser).</summary>
        public Guid PlayerId { get; set; }

        public string Name { get; set; } = null!;
        public CharacterClass Class { get; set; }
        public int Level { get; set; } = 1;
        public int Experience { get; set; }
        public int Gold { get; set; }

        /// <summary>Unspent skill points available for manual stat allocation.</summary>
        public int SkillPoints { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>Optimistic concurrency token.</summary>
        public uint RowVersion { get; set; }

        // Navigation properties
        public AppUser Player { get; set; } = null!;
        public CharacterStats Stats { get; set; } = null!;
        public ICollection<LearnedSkill> LearnedSkills { get; set; } = new List<LearnedSkill>();
        public ICollection<InventoryItem> Inventory { get; set; } = new List<InventoryItem>();
        public ICollection<EquippedItem> EquippedItems { get; set; } = new List<EquippedItem>();
        public ICollection<CharacterQuest> Quests { get; set; } = new List<CharacterQuest>();
    }
}
