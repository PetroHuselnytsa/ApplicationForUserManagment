using TestFirstProject.Models.Enums;

namespace TestFirstProject.Models.Game
{
    /// <summary>
    /// Enemy template (seeded data). Instances appear in battles via BattleParticipant.
    /// </summary>
    public class Enemy
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public Guid ZoneId { get; set; }
        public bool IsBoss { get; set; }
        public BossMechanic BossMechanic { get; set; } = BossMechanic.None;
        public DamageType DamageType { get; set; } = DamageType.Physical;

        // Base stats (scaled by zone level)
        public int BaseHP { get; set; }
        public int BaseMP { get; set; }
        public int BaseAttack { get; set; }
        public int BaseDefense { get; set; }
        public int BaseMagicPower { get; set; }
        public int BaseSpeed { get; set; }
        public double BaseCritChance { get; set; }
        public double BaseDodgeChance { get; set; }

        /// <summary>XP awarded on defeat.</summary>
        public int ExperienceReward { get; set; }

        /// <summary>Gold awarded on defeat.</summary>
        public int GoldReward { get; set; }

        /// <summary>AI strategy identifier for the enemy (e.g., "Aggressive", "Defensive", "Healer").</summary>
        public string AIStrategy { get; set; } = "Default";

        // Navigation
        public Zone Zone { get; set; } = null!;
        public ICollection<LootTableEntry> LootTable { get; set; } = new List<LootTableEntry>();
        public ICollection<EnemySkill> Skills { get; set; } = new List<EnemySkill>();
    }
}
