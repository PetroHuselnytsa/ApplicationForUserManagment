using TestFirstProject.Models.Enums;

namespace TestFirstProject.Models.Game
{
    /// <summary>
    /// Tracks quest progress per character.
    /// </summary>
    public class CharacterQuest
    {
        public Guid Id { get; set; }
        public Guid CharacterId { get; set; }
        public Guid QuestId { get; set; }
        public QuestStatus Status { get; set; } = QuestStatus.Active;

        /// <summary>Current progress toward the quest goal.</summary>
        public int Progress { get; set; }

        public DateTime AcceptedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }

        // Navigation
        public Character Character { get; set; } = null!;
        public Quest Quest { get; set; } = null!;
    }
}
