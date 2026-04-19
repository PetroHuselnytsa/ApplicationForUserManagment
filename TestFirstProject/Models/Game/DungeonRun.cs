using TestFirstProject.Models.Game.Enums;

namespace TestFirstProject.Models.Game
{
    /// <summary>
    /// A persisted dungeon run — a 5-room sequence through a zone.
    /// Player progresses room by room; state is saved between sessions.
    /// </summary>
    public class DungeonRun
    {
        public Guid Id { get; set; }
        public Guid CharacterId { get; set; }
        public Character Character { get; set; } = null!;

        public Guid ZoneId { get; set; }
        public Zone Zone { get; set; } = null!;

        public DungeonRunStatus Status { get; set; } = DungeonRunStatus.InProgress;
        public int CurrentRoomIndex { get; set; } = 0;

        public DateTime StartedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }

        // Navigation
        public ICollection<DungeonRoom> Rooms { get; set; } = new List<DungeonRoom>();
    }
}
