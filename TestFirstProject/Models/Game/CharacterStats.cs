namespace TestFirstProject.Models.Game
{
    /// <summary>
    /// Core combat stats for a character. Updated on level-up and by equipment bonuses.
    /// </summary>
    public class CharacterStats
    {
        public Guid Id { get; set; }
        public Guid CharacterId { get; set; }

        public int HP { get; set; }
        public int MaxHP { get; set; }
        public int MP { get; set; }
        public int MaxMP { get; set; }
        public int Attack { get; set; }
        public int Defense { get; set; }
        public int MagicPower { get; set; }
        public int Speed { get; set; }

        /// <summary>Probability 0-100 representing critical hit chance.</summary>
        public double CritChance { get; set; }

        /// <summary>Probability 0-100 representing dodge chance.</summary>
        public double DodgeChance { get; set; }

        // Navigation
        public Character Character { get; set; } = null!;
    }
}
