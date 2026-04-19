namespace TestFirstProject.Models.Game
{
    /// <summary>
    /// A skill that a character has learned. Tracks cooldown state during combat.
    /// </summary>
    public class LearnedSkill
    {
        public Guid Id { get; set; }
        public Guid CharacterId { get; set; }
        public Character Character { get; set; } = null!;

        public Guid SkillId { get; set; }
        public Skill Skill { get; set; } = null!;

        /// <summary>Remaining cooldown turns (0 = ready to use).</summary>
        public int CurrentCooldown { get; set; } = 0;
    }
}
