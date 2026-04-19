namespace TestFirstProject.Models.Game
{
    /// <summary>
    /// Junction entity: a skill learned by a character.
    /// </summary>
    public class LearnedSkill
    {
        public Guid Id { get; set; }
        public Guid CharacterId { get; set; }
        public Guid SkillId { get; set; }
        public DateTime LearnedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public Character Character { get; set; } = null!;
        public Skill Skill { get; set; } = null!;
    }
}
