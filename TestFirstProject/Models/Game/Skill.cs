using TestFirstProject.Models.Enums;

namespace TestFirstProject.Models.Game
{
    /// <summary>
    /// Definition of a skill available to a character class.
    /// Seeded as reference data.
    /// </summary>
    public class Skill
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public CharacterClass Class { get; set; }
        public SkillType Type { get; set; }
        public DamageType DamageType { get; set; }
        public TargetType TargetType { get; set; }

        /// <summary>Character level required to unlock this skill.</summary>
        public int UnlockLevel { get; set; }

        /// <summary>MP cost per use (Active skills only).</summary>
        public int ManaCost { get; set; }

        /// <summary>Cooldown in turns before the skill can be used again.</summary>
        public int Cooldown { get; set; }

        /// <summary>Base damage multiplier used in the damage formula.</summary>
        public double DamageMultiplier { get; set; }

        /// <summary>Status effect this skill applies, if any.</summary>
        public StatusEffectType? AppliesEffect { get; set; }

        /// <summary>Duration of the applied status effect in turns.</summary>
        public int EffectDuration { get; set; }

        /// <summary>Tick damage/heal value per turn for the status effect.</summary>
        public int EffectTickValue { get; set; }

        /// <summary>For passive skills: stat bonus type (e.g., "Attack", "Defense").</summary>
        public string? PassiveBonusStat { get; set; }

        /// <summary>For passive skills: amount of the bonus.</summary>
        public int PassiveBonusValue { get; set; }

        // Navigation
        public ICollection<LearnedSkill> LearnedBy { get; set; } = new List<LearnedSkill>();
        public ICollection<EnemySkill> EnemySkills { get; set; } = new List<EnemySkill>();
    }
}
