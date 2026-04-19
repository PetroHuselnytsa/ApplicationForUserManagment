using TestFirstProject.Models.Enums;

namespace TestFirstProject.Models.Game
{
    /// <summary>
    /// A single room in a dungeon run. Types: Combat, Treasure, Rest, Trap.
    /// </summary>
    public class DungeonRoom
    {
        public Guid Id { get; set; }
        public Guid DungeonRunId { get; set; }
        public int RoomIndex { get; set; }
        public RoomType Type { get; set; }
        public bool IsCompleted { get; set; }

        /// <summary>For Combat rooms: the enemy encountered.</summary>
        public Guid? EnemyId { get; set; }

        /// <summary>For Treasure rooms: the item found.</summary>
        public Guid? TreasureItemId { get; set; }

        /// <summary>For Trap rooms: damage dealt to the character.</summary>
        public int TrapDamage { get; set; }

        /// <summary>For Rest rooms: HP restored as percentage (e.g., 25).</summary>
        public int RestHealPercent { get; set; }

        /// <summary>For Combat rooms: link to the battle.</summary>
        public Guid? BattleId { get; set; }

        // Navigation
        public DungeonRun DungeonRun { get; set; } = null!;
        public Enemy? Enemy { get; set; }
        public Item? TreasureItem { get; set; }
        public Battle? Battle { get; set; }
    }
}
