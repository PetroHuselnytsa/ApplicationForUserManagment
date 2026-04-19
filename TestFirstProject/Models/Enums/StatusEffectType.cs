namespace TestFirstProject.Models.Enums
{
    /// <summary>
    /// Status effects that can be applied during combat.
    /// Each has a duration, tick value, and stack limit.
    /// </summary>
    public enum StatusEffectType
    {
        Burn,
        Poison,
        Stun,
        Bleed,
        Shield,
        Regen,
        Haste,
        Slow
    }
}
