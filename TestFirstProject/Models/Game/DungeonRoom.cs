using TestFirstProject.Models.Game.Enums;

namespace TestFirstProject.Models.Game
{
    /// <summary>
    /// A single room within a dungeon run. Room 5 is always the boss room.
    /// </summary>
    public class DungeonRoom
    {
        public Guid Id { get; set; }
        public Guid DungeonRunId { get; set; }
        public DungeonRun DungeonRun { get; set; } = null!;

        public int RoomIndex { get; set; }
        public RoomType Type { get; set; }
        public DungeonRoomStatus Status { get; set; } = DungeonRoomStatus.Pending;

        // For Combat rooms — the enemy to fight
        public Guid? EnemyId { get; set; }
        public Enemy? Enemy { get; set; }

        // For Treasure rooms — the item reward
        public Guid? TreasureItemId { get; set; }
        public Item? TreasureItem { get; set; }

        // For Trap rooms — damage dealt to character
        public int TrapDamage { get; set; }

        // For Rest rooms — HP/MP restored (percentage)
        public int RestorePercent { get; set; }

        // Associated battle (for combat rooms)
        public Guid? BattleId { get; set; }
        public Battle? Battle { get; set; }
    }
}
