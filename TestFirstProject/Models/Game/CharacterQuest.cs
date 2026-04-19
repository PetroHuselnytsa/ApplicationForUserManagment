using TestFirstProject.Models.Game.Enums;

namespace TestFirstProject.Models.Game
{
    /// <summary>
    /// Tracks a character's progress on a specific quest.
    /// </summary>
    public class CharacterQuest
    {
        public Guid Id { get; set; }
        public Guid CharacterId { get; set; }
        public Character Character { get; set; } = null!;

        public Guid QuestId { get; set; }
        public Quest Quest { get; set; } = null!;

        public QuestStatus Status { get; set; } = QuestStatus.Active;
        public int CurrentCount { get; set; } = 0;

        public DateTime AcceptedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }
        public DateTime? Deadline { get; set; }
    }
}
