using TestFirstProject.Models.Game;
using TestFirstProject.Models.Game.Enums;

namespace TestFirstProject.Services.Interfaces.Game
{
    /// <summary>
    /// Single responsibility: all damage calculations go through this service.
    /// Never compute damage inline — always use this calculator.
    /// </summary>
    public interface IDamageCalculator
    {
        /// <summary>Calculate basic attack damage between two participants.</summary>
        DamageResult CalculateBasicAttack(BattleParticipant attacker, BattleParticipant defender);

        /// <summary>Calculate skill damage from attacker to defender.</summary>
        DamageResult CalculateSkillDamage(BattleParticipant attacker, BattleParticipant defender, Skill skill);

        /// <summary>Check if an attack is dodged based on defender's dodge chance.</summary>
        bool RollDodge(BattleParticipant defender);

        /// <summary>Check if an attack is a critical hit.</summary>
        bool RollCritical(BattleParticipant attacker);
    }

    public class DamageResult
    {
        public int Damage { get; set; }
        public bool IsCritical { get; set; }
        public bool IsDodged { get; set; }
        public DamageType DamageType { get; set; }
    }
}
