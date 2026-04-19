using TestFirstProject.Models.Game;
using TestFirstProject.Models.Game.Enums;
using TestFirstProject.Services.Interfaces.Game;

namespace TestFirstProject.Services.Implementations.Game
{
    /// <summary>
    /// Single source of truth for all damage calculations.
    /// Formula: (Attack - Defense) * multiplier + critical_bonus
    /// </summary>
    public class DamageCalculator : IDamageCalculator
    {
        private static readonly Random _rng = new();

        public DamageResult CalculateBasicAttack(BattleParticipant attacker, BattleParticipant defender)
        {
            var result = new DamageResult { DamageType = DamageType.Physical };

            // Check dodge first
            if (RollDodge(defender))
            {
                result.IsDodged = true;
                result.Damage = 0;
                return result;
            }

            // Base damage: (Attack - Defense), minimum 1
            var baseDamage = Math.Max(attacker.Attack - defender.Defense, 1);

            // Check critical hit
            result.IsCritical = RollCritical(attacker);

            // Critical bonus: 50% extra damage
            var critMultiplier = result.IsCritical ? 1.5 : 1.0;

            // Add some variance (+/- 10%)
            var variance = 0.9 + _rng.NextDouble() * 0.2;

            result.Damage = Math.Max((int)(baseDamage * critMultiplier * variance), 1);
            return result;
        }

        public DamageResult CalculateSkillDamage(BattleParticipant attacker, BattleParticipant defender, Skill skill)
        {
            var result = new DamageResult { DamageType = skill.DamageType };

            // Self-target skills with 0 base damage are buffs, not damage
            if (skill.TargetType == SkillTargetType.Self && skill.BaseDamage == 0)
            {
                result.Damage = 0;
                return result;
            }

            // Check dodge
            if (RollDodge(defender))
            {
                result.IsDodged = true;
                result.Damage = 0;
                return result;
            }

            // Choose scaling stat based on damage type
            var scalingStat = skill.DamageType switch
            {
                DamageType.Physical => attacker.Attack,
                _ => attacker.MagicPower // All magic types scale off MagicPower
            };

            // Base damage from skill + (scaling_stat * multiplier)
            var rawDamage = skill.BaseDamage + (int)(scalingStat * skill.DamageMultiplier);

            // Subtract defense (physical) or half defense (magic)
            var effectiveDefense = skill.DamageType == DamageType.Physical
                ? defender.Defense
                : defender.Defense / 2;

            var netDamage = Math.Max(rawDamage - effectiveDefense, 1);

            // Critical hit check
            result.IsCritical = RollCritical(attacker);
            var critMultiplier = result.IsCritical ? 1.5 : 1.0;

            // Variance
            var variance = 0.9 + _rng.NextDouble() * 0.2;

            result.Damage = Math.Max((int)(netDamage * critMultiplier * variance), 1);
            return result;
        }

        public bool RollDodge(BattleParticipant defender)
        {
            return _rng.NextDouble() < defender.DodgeChance;
        }

        public bool RollCritical(BattleParticipant attacker)
        {
            return _rng.NextDouble() < attacker.CritChance;
        }
    }
}
