using Microsoft.EntityFrameworkCore;
using TestFirstProject.Contexts;
using TestFirstProject.Models.Enums;
using TestFirstProject.Models.Game;

namespace TestFirstProject.Services.Game.Implementations
{
    /// <summary>
    /// Processes all status effects: apply new effects, tick existing effects, expire finished effects.
    /// All status effect logic flows through this single processor.
    /// </summary>
    public class StatusEffectProcessor
    {
        private readonly PersonsContext _db;

        private static readonly Dictionary<StatusEffectType, int> StackLimits = new()
        {
            { StatusEffectType.Burn, 3 },
            { StatusEffectType.Poison, 5 },
            { StatusEffectType.Stun, 1 },
            { StatusEffectType.Bleed, 3 },
            { StatusEffectType.Shield, 1 },
            { StatusEffectType.Regen, 2 },
            { StatusEffectType.Haste, 1 },
            { StatusEffectType.Slow, 1 }
        };

        public StatusEffectProcessor(PersonsContext db)
        {
            _db = db;
        }

        /// <summary>
        /// Respects stack limits: if at max stacks, refreshes duration instead of adding.
        /// </summary>
        public async Task ApplyEffectAsync(Guid participantId, StatusEffectType type, int duration, int tickValue)
        {
            var existing = await _db.Set<ActiveStatusEffect>()
                .Where(e => e.BattleParticipantId == participantId && e.Type == type)
                .FirstOrDefaultAsync();

            var stackLimit = StackLimits.GetValueOrDefault(type, 1);

            if (existing != null)
            {
                if (existing.StackCount < stackLimit)
                {
                    existing.StackCount++;
                    existing.TickValue += tickValue;
                }
                existing.RemainingTurns = duration;
            }
            else
            {
                _db.Set<ActiveStatusEffect>().Add(new ActiveStatusEffect
                {
                    Id = Guid.NewGuid(),
                    BattleParticipantId = participantId,
                    Type = type,
                    RemainingTurns = duration,
                    TickValue = tickValue,
                    StackCount = 1
                });
            }
        }

        /// <summary>
        /// Process effects, check stun, and compute speed modifier in a single DB query.
        /// Returns a result that the caller can inspect without additional DB roundtrips.
        /// </summary>
        public async Task<TurnEffectResult> ProcessTurnEffectsAsync(BattleParticipant participant)
        {
            var descriptions = new List<string>();
            var effects = await _db.Set<ActiveStatusEffect>()
                .Where(e => e.BattleParticipantId == participant.Id)
                .ToListAsync();

            // Derive stun and speed modifiers from the loaded effects (no extra queries)
            bool isStunned = effects.Any(e => e.Type == StatusEffectType.Stun);

            int speedModifier = 0;
            foreach (var effect in effects.Where(e => e.Type == StatusEffectType.Haste || e.Type == StatusEffectType.Slow))
            {
                speedModifier += effect.Type == StatusEffectType.Haste
                    ? (int)(participant.Speed * 0.3)
                    : -(int)(participant.Speed * 0.3);
            }

            // Process each effect
            foreach (var effect in effects)
            {
                var desc = ProcessSingleEffect(participant, effect);
                if (desc != null)
                    descriptions.Add(desc);

                effect.RemainingTurns--;
            }

            // Remove expired effects
            var expired = effects.Where(e => e.RemainingTurns <= 0).ToList();
            if (expired.Count > 0)
            {
                _db.Set<ActiveStatusEffect>().RemoveRange(expired);
                foreach (var e in expired)
                {
                    descriptions.Add($"{participant.Name}'s {e.Type} effect has worn off.");
                }
            }

            return new TurnEffectResult(descriptions, isStunned, speedModifier);
        }

        /// <summary>
        /// Get shield absorption value if any shield effect is active.
        /// </summary>
        public async Task<int> GetShieldAbsorptionAsync(Guid participantId)
        {
            var shield = await _db.Set<ActiveStatusEffect>()
                .FirstOrDefaultAsync(e => e.BattleParticipantId == participantId && e.Type == StatusEffectType.Shield);

            return shield == null ? 0 : shield.TickValue * shield.StackCount;
        }

        private static string? ProcessSingleEffect(BattleParticipant participant, ActiveStatusEffect effect)
        {
            switch (effect.Type)
            {
                case StatusEffectType.Burn:
                case StatusEffectType.Poison:
                case StatusEffectType.Bleed:
                    var dot = effect.TickValue * effect.StackCount;
                    participant.CurrentHP = Math.Max(0, participant.CurrentHP - dot);
                    if (participant.CurrentHP <= 0) participant.IsAlive = false;
                    return $"{participant.Name} takes {dot} {effect.Type} damage. (HP: {participant.CurrentHP}/{participant.MaxHP})";

                case StatusEffectType.Regen:
                    var heal = effect.TickValue * effect.StackCount;
                    participant.CurrentHP = Math.Min(participant.MaxHP, participant.CurrentHP + heal);
                    return $"{participant.Name} regenerates {heal} HP. (HP: {participant.CurrentHP}/{participant.MaxHP})";

                case StatusEffectType.Shield:
                    return null;

                case StatusEffectType.Stun:
                    return $"{participant.Name} is stunned and cannot act!";

                case StatusEffectType.Haste:
                case StatusEffectType.Slow:
                    return null;

                default:
                    return null;
            }
        }
    }

    /// <summary>
    /// Result of processing turn effects — includes stun state and speed modifier
    /// so the caller doesn't need to make separate DB queries.
    /// </summary>
    public record TurnEffectResult(List<string> Descriptions, bool IsStunned, int SpeedModifier);
}
