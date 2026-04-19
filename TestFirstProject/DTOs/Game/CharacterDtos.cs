namespace TestFirstProject.DTOs.Game
{
    // --- Character DTOs ---

    public record CreateCharacterRequest(
        string Name,
        string CharacterClass
    );

    public record AllocateStatsRequest(
        int Hp,
        int Mp,
        int Attack,
        int Defense,
        int MagicPower,
        int Speed
    );

    public record CharacterDto(
        Guid Id,
        string Name,
        string CharacterClass,
        int Level,
        int Experience,
        int ExperienceToNextLevel,
        int SkillPoints,
        int CurrentHp,
        int CurrentMp,
        int Gold,
        DateTime CreatedAt
    );

    public record CharacterStatsDto(
        int BaseHp,
        int BaseMp,
        int BaseAttack,
        int BaseDefense,
        int BaseMagicPower,
        int BaseSpeed,
        double BaseCritChance,
        double BaseDodgeChance,
        int BonusHp,
        int BonusMp,
        int BonusAttack,
        int BonusDefense,
        int BonusMagicPower,
        int BonusSpeed,
        int TotalHp,
        int TotalMp,
        int TotalAttack,
        int TotalDefense,
        int TotalMagicPower,
        int TotalSpeed,
        List<EquippedItemDto> EquippedItems,
        List<LearnedSkillDto> LearnedSkills
    );

    public record EquippedItemDto(
        string Slot,
        string ItemName,
        string Rarity,
        int EnchantmentLevel
    );

    public record LearnedSkillDto(
        Guid SkillId,
        string Name,
        string Description,
        string Type,
        string TargetType,
        int MpCost,
        int CooldownTurns,
        int CurrentCooldown,
        int UnlockLevel
    );
}
