using Microsoft.EntityFrameworkCore;
using TestFirstProject.Contexts;
using TestFirstProject.Models.Game;
using TestFirstProject.Models.Game.Enums;
using TestFirstProject.Services.Interfaces.Game;

namespace TestFirstProject.Services.Implementations.Game
{
    /// <summary>
    /// Single processor for all status effect logic.
    /// Handles: Burn, Poison, Stun, Bleed, Shield, Regen, Haste, Slow.
    /// </summary>
    public class StatusEffectProcessor : IStatusEffectProcessor
    {
        private readonly GameDbContext _db;

        public StatusEffectProcessor(GameDbContext db)
        {
            _db = db;
        }

        public async Task<ActiveStatusEffect?> ApplyEffectAsync(Guid battleId, Guid targetParticipantId, StatusEffectType type, int duration, int tickValue, int stackLimit)
        {
            // Check for existing stacks of this effect
            var existing = await _db.ActiveStatusEffects
                .FirstOrDefaultAsync(e => e.BattleId == battleId
                    && e.TargetParticipantId == targetParticipantId
                    && e.Type == type);

            if (existing != null)
            {
                if (existing.Stacks < stackLimit)
                {
                    existing.Stacks++;
                    existing.RemainingTurns = Math.Max(existing.RemainingTurns, duration); // Refresh duration
                    existing.TickValue = tickValue; // Use latest tick value
                    await _db.SaveChangesAsync();
                    return existing;
                }
                // Already at max stacks — just refresh duration
                existing.RemainingTurns = Math.Max(existing.RemainingTurns, duration);
                await _db.SaveChangesAsync();
                return existing;
            }

            var effect = new ActiveStatusEffect
            {
                Id = Guid.NewGuid(),
                BattleId = battleId,
                TargetParticipantId = targetParticipantId,
                Type = type,
                RemainingTurns = duration,
                TickValue = tickValue,
                Stacks = 1
            };

            _db.ActiveStatusEffects.Add(effect);
            await _db.SaveChangesAsync();
            return effect;
        }

        public async Task<List<BattleLogEntry>> ProcessTurnStartEffectsAsync(Guid battleId, BattleParticipant participant)
        {
            var log = new List<BattleLogEntry>();
            var effects = await _db.ActiveStatusEffects
                .Where(e => e.BattleId == battleId && e.TargetParticipantId == participant.Id)
                .ToListAsync();

            foreach (var effect in effects)
            {
                var totalTick = effect.TickValue * effect.Stacks;

                switch (effect.Type)
                {
                    case StatusEffectType.Burn:
                    case StatusEffectType.Poison:
                    case StatusEffectType.Bleed:
                        // Damage over time
                        participant.CurrentHp = Math.Max(participant.CurrentHp - totalTick, 0);
                        log.Add(new BattleLogEntry
                        {
                            Actor = "Status Effect",
                            Action = effect.Type.ToString(),
                            Target = participant.Name,
                            Damage = totalTick,
                            Message = $"{participant.Name} takes {totalTick} {effect.Type} damage ({effect.Stacks} stack(s))."
                        });
                        break;

                    case StatusEffectType.Regen:
                        // Healing over time
                        var healAmount = Math.Min(totalTick, participant.MaxHp - participant.CurrentHp);
                        participant.CurrentHp += healAmount;
                        log.Add(new BattleLogEntry
                        {
                            Actor = "Status Effect",
                            Action = "Regen",
                            Target = participant.Name,
                            Healing = healAmount,
                            Message = $"{participant.Name} regenerates {healAmount} HP."
                        });
                        break;

                    case StatusEffectType.Haste:
                        // Speed boost is applied in turn order calculation
                        log.Add(new BattleLogEntry
                        {
                            Actor = "Status Effect",
                            Action = "Haste",
                            Target = participant.Name,
                            Message = $"{participant.Name} is hastened."
                        });
                        break;

                    case StatusEffectType.Slow:
                        log.Add(new BattleLogEntry
                        {
                            Actor = "Status Effect",
                            Action = "Slow",
                            Target = participant.Name,
                            Message = $"{participant.Name} is slowed."
                        });
                        break;

                    case StatusEffectType.Shield:
                        // Shield absorbs damage — handled in combat engine
                        log.Add(new BattleLogEntry
                        {
                            Actor = "Status Effect",
                            Action = "Shield",
                            Target = participant.Name,
                            Message = $"{participant.Name} is protected by a shield ({effect.TickValue} remaining)."
                        });
                        break;

                    case StatusEffectType.Stun:
                        log.Add(new BattleLogEntry
                        {
                            Actor = "Status Effect",
                            Action = "Stun",
                            Target = participant.Name,
                            Message = $"{participant.Name} is stunned and cannot act!"
                        });
                        break;
                }
            }

            await _db.SaveChangesAsync();
            return log;
        }

        public async Task<bool> IsStunnedAsync(Guid battleId, Guid participantId)
        {
            return await _db.ActiveStatusEffects
                .AnyAsync(e => e.BattleId == battleId
                    && e.TargetParticipantId == participantId
                    && e.Type == StatusEffectType.Stun
                    && e.RemainingTurns > 0);
        }

        public async Task TickEffectsAsync(Guid battleId, Guid participantId)
        {
            var effects = await _db.ActiveStatusEffects
                .Where(e => e.BattleId == battleId && e.TargetParticipantId == participantId)
                .ToListAsync();

            foreach (var effect in effects)
            {
                effect.RemainingTurns--;
                if (effect.RemainingTurns <= 0)
                {
                    _db.ActiveStatusEffects.Remove(effect);
                }
            }

            await _db.SaveChangesAsync();
        }

        public async Task<int> GetShieldValueAsync(Guid battleId, Guid participantId)
        {
            var shield = await _db.ActiveStatusEffects
                .FirstOrDefaultAsync(e => e.BattleId == battleId
                    && e.TargetParticipantId == participantId
                    && e.Type == StatusEffectType.Shield);

            return shield?.TickValue ?? 0;
        }

        public async Task ReduceShieldAsync(Guid battleId, Guid participantId, int damageAbsorbed)
        {
            var shield = await _db.ActiveStatusEffects
                .FirstOrDefaultAsync(e => e.BattleId == battleId
                    && e.TargetParticipantId == participantId
                    && e.Type == StatusEffectType.Shield);

            if (shield == null) return;

            shield.TickValue -= damageAbsorbed;
            if (shield.TickValue <= 0)
            {
                _db.ActiveStatusEffects.Remove(shield);
            }

            await _db.SaveChangesAsync();
        }
    }
}
