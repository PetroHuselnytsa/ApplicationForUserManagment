namespace TestFirstProject.Models.Enums
{
    /// <summary>
    /// Skill activation types.
    /// </summary>
    public enum SkillType
    {
        /// <summary>Costs MP, used in combat by submitting a UseSkill action.</summary>
        Active,

        /// <summary>Always applied as a passive bonus to stats.</summary>
        Passive
    }
}
