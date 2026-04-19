using TestFirstProject.Models.Game.Enums;

namespace TestFirstProject.Models.Game
{
    /// <summary>
    /// Enemy template. Stats scale based on zone level. Bosses have unique mechanics.
    /// </summary>
    public class Enemy
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;

        public bool IsBoss { get; set; } = false;
        public BossMechanic BossMechanic { get; set; } = BossMechanic.None;

        // Base stats (scaled by zone level at encounter time)
        public int BaseHp { get; set; }
        public int BaseMp { get; set; }
        public int BaseAttack { get; set; }
        public int BaseDefense { get; set; }
        public int BaseMagicPower { get; set; }
        public int BaseSpeed { get; set; }
        public double BaseCritChance { get; set; }
        public double BaseDodgeChance { get; set; }

        public DamageType PrimaryDamageType { get; set; } = DamageType.Physical;

        // XP and gold awarded on defeat
        public int BaseXpReward { get; set; }
        public int BaseGoldReward { get; set; }

        // Zone association
        public Guid? ZoneId { get; set; }
        public Zone? Zone { get; set; }

        // Navigation
        public ICollection<LootTableEntry> LootTable { get; set; } = new List<LootTableEntry>();

        // Enemy skills (for AI)
        public ICollection<Skill> Skills { get; set; } = new List<Skill>();
    }
}
