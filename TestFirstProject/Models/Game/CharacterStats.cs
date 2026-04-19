namespace TestFirstProject.Models.Game
{
    /// <summary>
    /// Full stat sheet for a character. Base stats scale with class and level;
    /// bonus stats come from equipment and manual allocation.
    /// </summary>
    public class CharacterStats
    {
        public Guid Id { get; set; }
        public Guid CharacterId { get; set; }
        public Character Character { get; set; } = null!;

        // Base stats (determined by class + level)
        public int BaseHp { get; set; }
        public int BaseMp { get; set; }
        public int BaseAttack { get; set; }
        public int BaseDefense { get; set; }
        public int BaseMagicPower { get; set; }
        public int BaseSpeed { get; set; }
        public double BaseCritChance { get; set; }
        public double BaseDodgeChance { get; set; }

        // Bonus stats from skill point allocation
        public int BonusHp { get; set; }
        public int BonusMp { get; set; }
        public int BonusAttack { get; set; }
        public int BonusDefense { get; set; }
        public int BonusMagicPower { get; set; }
        public int BonusSpeed { get; set; }

        // Computed totals (base + bonus; equipment bonuses added at runtime)
        public int TotalHp => BaseHp + BonusHp;
        public int TotalMp => BaseMp + BonusMp;
        public int TotalAttack => BaseAttack + BonusAttack;
        public int TotalDefense => BaseDefense + BonusDefense;
        public int TotalMagicPower => BaseMagicPower + BonusMagicPower;
        public int TotalSpeed => BaseSpeed + BonusSpeed;
    }
}
