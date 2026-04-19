using TestFirstProject.Models.Enums;

namespace TestFirstProject.Models.Game
{
    /// <summary>
    /// A persisted dungeon run: 5-room sequence with random encounters and a fixed boss room.
    /// </summary>
    public class DungeonRun
    {
        public Guid Id { get; set; }
        public Guid CharacterId { get; set; }
        public Guid ZoneId { get; set; }
        public DungeonRunStatus Status { get; set; } = DungeonRunStatus.InProgress;
        public int CurrentRoomIndex { get; set; }
        public DateTime StartedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }

        // Navigation
        public Character Character { get; set; } = null!;
        public Zone Zone { get; set; } = null!;
        public ICollection<DungeonRoom> Rooms { get; set; } = new List<DungeonRoom>();
    }
}
