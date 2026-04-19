using TestFirstProject.Models.Game.Enums;

namespace TestFirstProject.Models.Game
{
    /// <summary>
    /// Definition of a skill available to a character class.
    /// Seed data — not created by players.
    /// </summary>
    public class Skill
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;

        public CharacterClass Class { get; set; }
        public SkillType Type { get; set; }
        public int UnlockLevel { get; set; }

        // Active skill properties
        public int MpCost { get; set; }
        public int CooldownTurns { get; set; }
        public SkillTargetType TargetType { get; set; }
        public DamageType DamageType { get; set; }

        // Damage formula: base damage + (scaling_stat * multiplier)
        public int BaseDamage { get; set; }
        public double DamageMultiplier { get; set; } = 1.0;

        // Passive skill bonuses
        public int PassiveHpBonus { get; set; }
        public int PassiveMpBonus { get; set; }
        public int PassiveAttackBonus { get; set; }
        public int PassiveDefenseBonus { get; set; }
        public int PassiveMagicPowerBonus { get; set; }
        public int PassiveSpeedBonus { get; set; }

        // Status effect this skill can apply (null if none)
        public StatusEffectType? AppliesEffect { get; set; }
        public int EffectDuration { get; set; }
        public int EffectTickValue { get; set; }
        public int EffectStackLimit { get; set; } = 1;
    }
}
