using TestFirstProject.Models.Enums;
using TestFirstProject.Models.Game;

namespace TestFirstProject.Services.Game.Implementations
{
    /// <summary>
    /// Centralized damage calculation. All damage in the game flows through this single calculator.
    /// Formula: (Attack - Defense) * multiplier + critical_bonus
    /// Critical hits and dodges are probabilistic based on stats.
    /// </summary>
    public class DamageCalculator
    {
        public DamageResult CalculateBasicAttack(BattleParticipant attacker, BattleParticipant defender)
        {
            return CalculateDamage(attacker.Attack, defender.Defense, 1.0,
                attacker.CritChance, defender.DodgeChance);
        }

        public DamageResult CalculateSkillDamage(BattleParticipant attacker, BattleParticipant defender, Skill skill)
        {
            var attackStat = skill.DamageType == DamageType.Physical
                ? attacker.Attack
                : attacker.MagicPower;

            return CalculateDamage(attackStat, defender.Defense, skill.DamageMultiplier,
                attacker.CritChance, defender.DodgeChance);
        }

        /// <summary>
        /// Core damage formula: max(1, (attack - defense)) * multiplier + critical_bonus
        /// </summary>
        private static DamageResult CalculateDamage(int attackStat, int defenseStat, double multiplier,
            double critChance, double dodgeChance)
        {
            if (Random.Shared.NextDouble() * 100 < dodgeChance)
                return new DamageResult(0, false, true);

            var baseDamage = Math.Max(1, attackStat - defenseStat);
            var damage = (int)(baseDamage * multiplier);

            var isCrit = Random.Shared.NextDouble() * 100 < critChance;
            if (isCrit)
                damage += (int)(damage * 0.5);

            return new DamageResult(Math.Max(1, damage), isCrit, false);
        }

        public int CalculateHealing(int baseValue)
        {
            // Slight variance: 90-110% of base value
            var variance = 0.9 + (Random.Shared.NextDouble() * 0.2);
            return Math.Max(1, (int)(baseValue * variance));
        }
    }

    public record DamageResult(int Damage, bool IsCritical, bool IsDodged);
}
