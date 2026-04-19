using Microsoft.EntityFrameworkCore;
using TestFirstProject.Contexts;
using TestFirstProject.Exceptions;
using TestFirstProject.Models.Game;
using TestFirstProject.Models.Game.Enums;
using TestFirstProject.Services.Interfaces.Game;

namespace TestFirstProject.Services.Implementations.Game
{
    /// <summary>
    /// Manages dungeon runs: 5-room sequences with random encounters and a fixed boss room.
    /// Room types: Combat, Treasure, Rest, Trap. State is fully persisted.
    /// </summary>
    public class DungeonRunner : IDungeonRunner
    {
        private readonly GameDbContext _db;
        private readonly ICombatEngine _combatEngine;
        private readonly ILootService _lootService;
        private static readonly Random _rng = new();

        public DungeonRunner(GameDbContext db, ICombatEngine combatEngine, ILootService lootService)
        {
            _db = db;
            _combatEngine = combatEngine;
            _lootService = lootService;
        }

        public async Task<DungeonRun> StartDungeonAsync(Guid characterId, Guid zoneId)
        {
            var character = await _db.Characters.FindAsync(characterId)
                ?? throw new NotFoundException("Character not found.");

            var zone = await _db.Zones.FindAsync(zoneId)
                ?? throw new NotFoundException("Zone not found.");

            // Check character level meets zone requirements
            if (character.Level < zone.MinLevel)
                throw new ValidationException($"Character must be at least level {zone.MinLevel} for this zone.");

            // Check for active dungeon run
            var activeRun = await _db.DungeonRuns
                .FirstOrDefaultAsync(dr => dr.CharacterId == characterId && dr.Status == DungeonRunStatus.InProgress);
            if (activeRun != null)
                throw new ConflictException("Character already has an active dungeon run. Complete or abandon it first.");

            // Check for active battle
            var activeBattle = await _db.Battles
                .FirstOrDefaultAsync(b => b.CharacterId == characterId && b.Status == BattleStatus.InProgress);
            if (activeBattle != null)
                throw new ConflictException("Character has an active battle. Complete it first.");

            // Get available enemies for this zone
            var enemies = await _db.Enemies
                .Where(e => e.ZoneId == zoneId && !e.IsBoss)
                .ToListAsync();

            var boss = await _db.Enemies
                .FirstOrDefaultAsync(e => e.ZoneId == zoneId && e.IsBoss);

            // Get treasure items (common to rare items)
            var treasureItems = await _db.Items
                .Where(i => i.Type != ItemType.Material && i.LevelRequirement <= zone.MaxLevel)
                .OrderBy(i => i.Rarity)
                .Take(10)
                .ToListAsync();

            var run = new DungeonRun
            {
                Id = Guid.NewGuid(),
                CharacterId = characterId,
                ZoneId = zoneId,
                Status = DungeonRunStatus.InProgress,
                CurrentRoomIndex = 0
            };

            _db.DungeonRuns.Add(run);

            // Generate 5 rooms: rooms 0-3 are random, room 4 is always boss
            var roomTypes = new[] { RoomType.Combat, RoomType.Treasure, RoomType.Rest, RoomType.Trap };

            for (int i = 0; i < 5; i++)
            {
                var room = new DungeonRoom
                {
                    Id = Guid.NewGuid(),
                    DungeonRunId = run.Id,
                    RoomIndex = i,
                    Status = DungeonRoomStatus.Pending
                };

                if (i == 4)
                {
                    // Boss room
                    room.Type = RoomType.Combat;
                    room.EnemyId = boss?.Id;
                }
                else
                {
                    room.Type = roomTypes[_rng.Next(roomTypes.Length)];

                    switch (room.Type)
                    {
                        case RoomType.Combat:
                            if (enemies.Any())
                                room.EnemyId = enemies[_rng.Next(enemies.Count)].Id;
                            break;

                        case RoomType.Treasure:
                            if (treasureItems.Any())
                                room.TreasureItemId = treasureItems[_rng.Next(treasureItems.Count)].Id;
                            break;

                        case RoomType.Rest:
                            room.RestorePercent = _rng.Next(20, 40); // 20-39% restore
                            break;

                        case RoomType.Trap:
                            room.TrapDamage = _rng.Next(10, 30) + (zone.MinLevel * 2); // Scale with zone
                            break;
                    }
                }

                _db.DungeonRooms.Add(room);
            }

            await _db.SaveChangesAsync();
            return await GetDungeonRunAsync(run.Id);
        }

        public async Task<DungeonRun> GetDungeonRunAsync(Guid runId)
        {
            var run = await _db.DungeonRuns
                .Include(dr => dr.Zone)
                .Include(dr => dr.Rooms.OrderBy(r => r.RoomIndex))
                    .ThenInclude(r => r.Enemy)
                .Include(dr => dr.Rooms)
                    .ThenInclude(r => r.TreasureItem)
                .FirstOrDefaultAsync(dr => dr.Id == runId)
                ?? throw new NotFoundException("Dungeon run not found.");

            return run;
        }

        public async Task<DungeonRoomResult> ActInRoomAsync(Guid runId, Guid characterId, string action)
        {
            var run = await _db.DungeonRuns
                .Include(dr => dr.Rooms.OrderBy(r => r.RoomIndex))
                    .ThenInclude(r => r.Enemy)
                .Include(dr => dr.Rooms)
                    .ThenInclude(r => r.TreasureItem)
                .FirstOrDefaultAsync(dr => dr.Id == runId && dr.CharacterId == characterId)
                ?? throw new NotFoundException("Dungeon run not found.");

            if (run.Status != DungeonRunStatus.InProgress)
                throw new ValidationException("Dungeon run is not in progress.");

            var currentRoom = run.Rooms.FirstOrDefault(r => r.RoomIndex == run.CurrentRoomIndex)
                ?? throw new ValidationException("No room found at current index.");

            var result = new DungeonRoomResult
            {
                RoomType = currentRoom.Type.ToString(),
                Status = currentRoom.Status.ToString()
            };

            switch (action.ToLowerInvariant())
            {
                case "enter":
                    result = await EnterRoom(run, currentRoom, characterId);
                    break;

                case "proceed":
                    result = await ProceedToNextRoom(run, currentRoom);
                    break;

                case "abandon":
                    run.Status = DungeonRunStatus.Abandoned;
                    run.CompletedAt = DateTime.UtcNow;
                    result.Message = "Dungeon run abandoned.";
                    result.Status = "Abandoned";
                    break;

                default:
                    throw new ValidationException("Invalid action. Use 'enter', 'proceed', or 'abandon'.");
            }

            await _db.SaveChangesAsync();
            return result;
        }

        private async Task<DungeonRoomResult> EnterRoom(DungeonRun run, DungeonRoom room, Guid characterId)
        {
            if (room.Status != DungeonRoomStatus.Pending)
                throw new ValidationException("Room has already been entered. Use 'proceed' to advance.");

            room.Status = DungeonRoomStatus.InProgress;
            var result = new DungeonRoomResult { RoomType = room.Type.ToString() };

            switch (room.Type)
            {
                case RoomType.Combat:
                    if (room.EnemyId.HasValue)
                    {
                        var battle = await _combatEngine.StartBattleAsync(characterId, room.EnemyId.Value, room.Id);
                        room.BattleId = battle.Id;
                        result.BattleId = battle.Id;
                        result.Message = $"A {room.Enemy?.Name ?? "monster"} blocks your path! Battle started.";
                        result.Status = "InProgress";
                    }
                    else
                    {
                        room.Status = DungeonRoomStatus.Completed;
                        result.Message = "The room is empty. You may proceed.";
                        result.Status = "Completed";
                    }
                    break;

                case RoomType.Treasure:
                    if (room.TreasureItemId.HasValue && room.TreasureItem != null)
                    {
                        await _lootService.AddItemsToInventoryAsync(characterId, new List<DroppedItem>
                        {
                            new() { ItemId = room.TreasureItemId.Value, ItemName = room.TreasureItem.Name, Quantity = 1 }
                        });
                        result.TreasureItemName = room.TreasureItem.Name;
                        result.Message = $"You found a treasure chest containing {room.TreasureItem.Name}!";
                    }
                    else
                    {
                        result.Message = "The treasure chest is empty.";
                    }
                    room.Status = DungeonRoomStatus.Completed;
                    result.Status = "Completed";
                    break;

                case RoomType.Rest:
                    var character = await _db.Characters
                        .Include(c => c.Stats)
                        .FirstAsync(c => c.Id == characterId);

                    var hpRestore = (int)(character.Stats.TotalHp * room.RestorePercent / 100.0);
                    var mpRestore = (int)(character.Stats.TotalMp * room.RestorePercent / 100.0);

                    character.CurrentHp = Math.Min(character.CurrentHp + hpRestore, character.Stats.TotalHp);
                    character.CurrentMp = Math.Min(character.CurrentMp + mpRestore, character.Stats.TotalMp);

                    result.HpRestored = hpRestore;
                    result.MpRestored = mpRestore;
                    result.Message = $"You rest by a campfire. Restored {hpRestore} HP and {mpRestore} MP.";
                    room.Status = DungeonRoomStatus.Completed;
                    result.Status = "Completed";
                    break;

                case RoomType.Trap:
                    var trapChar = await _db.Characters.FindAsync(characterId)!;
                    trapChar!.CurrentHp = Math.Max(trapChar.CurrentHp - room.TrapDamage, 1); // Can't die to traps
                    result.TrapDamage = room.TrapDamage;
                    result.Message = $"You triggered a trap! Took {room.TrapDamage} damage.";
                    room.Status = DungeonRoomStatus.Completed;
                    result.Status = "Completed";
                    break;
            }

            return result;
        }

        private async Task<DungeonRoomResult> ProceedToNextRoom(DungeonRun run, DungeonRoom currentRoom)
        {
            // For combat rooms, check if battle is won
            if (currentRoom.Type == RoomType.Combat && currentRoom.BattleId.HasValue)
            {
                var battle = await _db.Battles.FindAsync(currentRoom.BattleId.Value);
                if (battle != null && battle.Status == BattleStatus.InProgress)
                    throw new ValidationException("You must complete the battle before proceeding.");

                if (battle != null && battle.Status == BattleStatus.Defeat)
                {
                    run.Status = DungeonRunStatus.Failed;
                    run.CompletedAt = DateTime.UtcNow;
                    return new DungeonRoomResult
                    {
                        RoomType = currentRoom.Type.ToString(),
                        Status = "Failed",
                        Message = "You were defeated. The dungeon run has failed."
                    };
                }

                currentRoom.Status = DungeonRoomStatus.Completed;
            }

            if (currentRoom.Status != DungeonRoomStatus.Completed)
                throw new ValidationException("Current room is not completed. Enter the room first.");

            // Check if dungeon is complete
            if (run.CurrentRoomIndex >= 4)
            {
                run.Status = DungeonRunStatus.Completed;
                run.CompletedAt = DateTime.UtcNow;
                return new DungeonRoomResult
                {
                    RoomType = currentRoom.Type.ToString(),
                    Status = "Completed",
                    Message = "Congratulations! You have completed the dungeon!",
                    IsDungeonComplete = true
                };
            }

            // Advance to next room
            run.CurrentRoomIndex++;
            var nextRoom = run.Rooms.FirstOrDefault(r => r.RoomIndex == run.CurrentRoomIndex);

            return new DungeonRoomResult
            {
                RoomType = nextRoom?.Type.ToString() ?? "Unknown",
                Status = "Pending",
                Message = $"You advance to room {run.CurrentRoomIndex + 1}/5. It appears to be a {nextRoom?.Type.ToString().ToLowerInvariant()} room."
            };
        }
    }
}
