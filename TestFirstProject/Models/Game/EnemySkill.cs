namespace TestFirstProject.Models.Game
{
    /// <summary>
    /// Junction: an enemy's available skill in combat.
    /// </summary>
    public class EnemySkill
    {
        public Guid Id { get; set; }
        public Guid EnemyId { get; set; }
        public Guid SkillId { get; set; }

        // Navigation
        public Enemy Enemy { get; set; } = null!;
        public Skill Skill { get; set; } = null!;
    }
}
