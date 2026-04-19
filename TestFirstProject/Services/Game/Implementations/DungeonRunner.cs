using Microsoft.EntityFrameworkCore;
using TestFirstProject.Contexts;
using TestFirstProject.DTOs.Game;
using TestFirstProject.Exceptions;
using TestFirstProject.Models.Enums;
using TestFirstProject.Models.Game;
using TestFirstProject.Services.Game.Interfaces;

namespace TestFirstProject.Services.Game.Implementations
{
    /// <summary>
    /// Dungeon run management: 5-room sequences with random encounters and a fixed boss room.
    /// Dungeon run state is persisted — player progresses room by room.
    /// </summary>
    public class DungeonRunner : IDungeonRunner
    {
        private readonly PersonsContext _db;
        private readonly ICombatEngine _combatEngine;
        private const int DungeonRoomCount = 5;

        public DungeonRunner(PersonsContext db, ICombatEngine combatEngine)
        {
            _db = db;
            _combatEngine = combatEngine;
        }

        public async Task<DungeonRunResponse> StartRunAsync(Guid playerId, Guid characterId, Guid zoneId)
        {
            var character = await _db.Characters
                .Include(c => c.Stats)
                .FirstOrDefaultAsync(c => c.Id == characterId && c.PlayerId == playerId)
                ?? throw new NotFoundException("Character not found.");

            var zone = await _db.Zones
                .Include(z => z.Enemies)
                .FirstOrDefaultAsync(z => z.Id == zoneId)
                ?? throw new NotFoundException("Zone not found.");

            if (character.Level < zone.MinLevel)
                throw new ValidationException($"Character level {character.Level} is below zone minimum level {zone.MinLevel}.");

            var activeRun = await _db.DungeonRuns
                .AnyAsync(r => r.CharacterId == characterId && r.Status == DungeonRunStatus.InProgress);
            if (activeRun)
                throw new ConflictException("Character already has an active dungeon run.");

            var activeBattle = await _db.Battles
                .AnyAsync(b => b.CharacterId == characterId && b.Status == BattleStatus.InProgress);
            if (activeBattle)
                throw new ConflictException("Character is currently in a battle.");

            var dungeonRun = new DungeonRun
            {
                Id = Guid.NewGuid(),
                CharacterId = characterId,
                ZoneId = zoneId,
                Status = DungeonRunStatus.InProgress,
                CurrentRoomIndex = 0,
                StartedAt = DateTime.UtcNow
            };

            var regularEnemies = zone.Enemies.Where(e => !e.IsBoss).ToList();
            var boss = zone.Enemies.FirstOrDefault(e => e.IsBoss);
            var roomTypes = new[] { RoomType.Combat, RoomType.Treasure, RoomType.Rest, RoomType.Trap };

            // Defer treasure item loading until we know we need it
            List<Item>? treasureItems = null;

            for (int i = 0; i < DungeonRoomCount; i++)
            {
                var room = new DungeonRoom
                {
                    Id = Guid.NewGuid(),
                    DungeonRunId = dungeonRun.Id,
                    RoomIndex = i,
                    IsCompleted = false
                };

                if (i == DungeonRoomCount - 1)
                {
                    room.Type = RoomType.Combat;
                    room.EnemyId = boss?.Id;
                }
                else
                {
                    room.Type = roomTypes[Random.Shared.Next(roomTypes.Length)];

                    switch (room.Type)
                    {
                        case RoomType.Combat:
                            if (regularEnemies.Count > 0)
                                room.EnemyId = regularEnemies[Random.Shared.Next(regularEnemies.Count)].Id;
                            break;
                        case RoomType.Treasure:
                            // Load treasure items only when first Treasure room is generated
                            treasureItems ??= await _db.Items
                                .Where(item => item.LevelRequirement <= zone.MaxLevel && item.Type != ItemType.Material)
                                .OrderBy(item => Guid.NewGuid())
                                .Take(3)
                                .ToListAsync();
                            if (treasureItems.Count > 0)
                                room.TreasureItemId = treasureItems[Random.Shared.Next(treasureItems.Count)].Id;
                            break;
                        case RoomType.Rest:
                            room.RestHealPercent = 25;
                            break;
                        case RoomType.Trap:
                            room.TrapDamage = (int)(character.Stats.MaxHP * 0.15);
                            break;
                    }
                }

                dungeonRun.Rooms.Add(room);
            }

            _db.DungeonRuns.Add(dungeonRun);
            await _db.SaveChangesAsync();

            return BuildRunResponse(dungeonRun);
        }

        public async Task<DungeonRunResponse> GetRunStateAsync(Guid playerId, Guid runId)
        {
            var run = await LoadRunWithGraph(runId);

            if (run.Character.PlayerId != playerId)
                throw new ForbiddenException("You do not have access to this dungeon run.");

            return BuildRunResponse(run);
        }

        public async Task<DungeonRunResponse> ActInRoomAsync(Guid playerId, Guid runId, DungeonActionRequest request)
        {
            var run = await _db.DungeonRuns
                .Include(r => r.Character).ThenInclude(c => c.Stats)
                .Include(r => r.Rooms).ThenInclude(rm => rm.Enemy)
                .Include(r => r.Zone)
                .FirstOrDefaultAsync(r => r.Id == runId)
                ?? throw new NotFoundException("Dungeon run not found.");

            if (run.Character.PlayerId != playerId)
                throw new ForbiddenException("You do not have access to this dungeon run.");

            if (run.Status != DungeonRunStatus.InProgress)
                throw new ValidationException($"Dungeon run is already {run.Status}.");

            var currentRoom = run.Rooms
                .OrderBy(r => r.RoomIndex)
                .FirstOrDefault(r => r.RoomIndex == run.CurrentRoomIndex)
                ?? throw new AppException("No room at current index.", 500);

            // Use case-insensitive comparison instead of ToLower() (avoids Turkish-I problem)
            if (string.Equals(request.Action, "enter", StringComparison.OrdinalIgnoreCase))
            {
                await EnterRoom(run, currentRoom, playerId);
            }
            else if (string.Equals(request.Action, "continue", StringComparison.OrdinalIgnoreCase))
            {
                if (!currentRoom.IsCompleted)
                    throw new ValidationException("Current room is not yet completed.");

                run.CurrentRoomIndex++;
                if (run.CurrentRoomIndex >= DungeonRoomCount)
                {
                    run.Status = DungeonRunStatus.Completed;
                    run.CompletedAt = DateTime.UtcNow;
                }
            }
            else if (string.Equals(request.Action, "flee", StringComparison.OrdinalIgnoreCase))
            {
                run.Status = DungeonRunStatus.Abandoned;
                run.CompletedAt = DateTime.UtcNow;
            }
            else
            {
                throw new ValidationException($"Invalid action: {request.Action}. Valid: Enter, Continue, Flee");
            }

            await _db.SaveChangesAsync();
            return BuildRunResponse(run);
        }

        #region Private Helpers

        private async Task EnterRoom(DungeonRun run, DungeonRoom room, Guid playerId)
        {
            if (room.IsCompleted)
                throw new ValidationException("This room is already completed.");

            switch (room.Type)
            {
                case RoomType.Combat:
                    if (room.EnemyId.HasValue && room.BattleId == null)
                    {
                        // Start a battle and use the returned BattleId directly
                        var battleState = await _combatEngine.StartBattleAsync(playerId, new StartBattleRequest(
                            run.CharacterId, room.EnemyId, null));

                        room.BattleId = battleState.BattleId;

                        var battle = await _db.Battles.FindAsync(battleState.BattleId);
                        if (battle != null)
                            battle.DungeonRoomId = room.Id;
                    }
                    else if (room.BattleId.HasValue)
                    {
                        var existingBattle = await _db.Battles.FindAsync(room.BattleId.Value);
                        if (existingBattle == null)
                            break;

                        switch (existingBattle.Status)
                        {
                            case BattleStatus.Victory:
                                room.IsCompleted = true;
                                break;
                            case BattleStatus.Defeat:
                                run.Status = DungeonRunStatus.Failed;
                                run.CompletedAt = DateTime.UtcNow;
                                break;
                            case BattleStatus.Fled:
                                // Fleeing from a dungeon combat counts as abandoning the run
                                run.Status = DungeonRunStatus.Abandoned;
                                run.CompletedAt = DateTime.UtcNow;
                                break;
                            case BattleStatus.InProgress:
                                throw new ValidationException(
                                    $"Battle {room.BattleId.Value} is still in progress. Complete it via the battle API before continuing.");
                        }
                    }
                    else
                    {
                        room.IsCompleted = true;
                    }
                    break;

                case RoomType.Treasure:
                    if (room.TreasureItemId.HasValue)
                    {
                        await InventoryHelper.AddOrIncrementAsync(_db, run.CharacterId, room.TreasureItemId.Value);
                    }
                    room.IsCompleted = true;
                    break;

                case RoomType.Rest:
                    var stats = run.Character.Stats;
                    var healAmount = (int)(stats.MaxHP * (room.RestHealPercent / 100.0));
                    stats.HP = Math.Min(stats.MaxHP, stats.HP + healAmount);
                    room.IsCompleted = true;
                    break;

                case RoomType.Trap:
                    var charStats = run.Character.Stats;
                    charStats.HP = Math.Max(1, charStats.HP - room.TrapDamage);
                    room.IsCompleted = true;
                    break;
            }
        }

        private async Task<DungeonRun> LoadRunWithGraph(Guid runId)
        {
            return await _db.DungeonRuns
                .Include(r => r.Character)
                .Include(r => r.Zone)
                .Include(r => r.Rooms).ThenInclude(rm => rm.Enemy)
                .FirstOrDefaultAsync(r => r.Id == runId)
                ?? throw new NotFoundException("Dungeon run not found.");
        }

        /// <summary>
        /// Build response from in-memory entity (no re-query).
        /// </summary>
        private static DungeonRunResponse BuildRunResponse(DungeonRun run)
        {
            var rooms = run.Rooms
                .OrderBy(r => r.RoomIndex)
                .Select(r => new DungeonRoomResponse(
                    r.RoomIndex,
                    r.Type.ToString(),
                    r.IsCompleted,
                    r.Enemy?.Name,
                    r.BattleId
                )).ToList();

            return new DungeonRunResponse(
                run.Id,
                run.Zone.Name,
                run.Status.ToString(),
                run.CurrentRoomIndex,
                DungeonRoomCount,
                rooms
            );
        }

        #endregion
    }
}
