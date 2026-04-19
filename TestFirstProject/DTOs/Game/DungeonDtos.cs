namespace TestFirstProject.DTOs.Game
{
    // --- Dungeon DTOs ---

    public record StartDungeonRequest(
        Guid CharacterId
    );

    public record DungeonRunDto(
        Guid Id,
        string ZoneName,
        string Status,
        int CurrentRoomIndex,
        int TotalRooms,
        List<DungeonRoomDto> Rooms,
        DateTime StartedAt,
        DateTime? CompletedAt
    );

    public record DungeonRoomDto(
        int RoomIndex,
        string Type,
        string Status,
        string? EnemyName,
        bool IsBoss
    );

    public record DungeonActionRequest(
        string Action
    );

    public record DungeonRoomResultDto(
        string RoomType,
        string Status,
        string Message,
        Guid? BattleId,
        int? TrapDamage,
        int? HpRestored,
        int? MpRestored,
        string? TreasureItemName,
        bool IsDungeonComplete
    );
}
