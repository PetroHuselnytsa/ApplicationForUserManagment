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
    /// Handles character creation, leveling, XP curves, and stat allocation.
    /// Each class has unique base stats and per-level scaling formulas.
    /// </summary>
    public class CharacterProgressionService : ICharacterProgressionService
    {
        private readonly PersonsContext _db;

        public CharacterProgressionService(PersonsContext db)
        {
            _db = db;
        }

        public async Task<CharacterResponse> CreateCharacterAsync(Guid playerId, CreateCharacterRequest request)
        {
            if (!Enum.TryParse<CharacterClass>(request.Class, true, out var charClass))
                throw new ValidationException($"Invalid character class: {request.Class}. Valid: Warrior, Mage, Rogue, Paladin");

            if (string.IsNullOrWhiteSpace(request.Name) || request.Name.Length > 50)
                throw new ValidationException("Character name must be 1-50 characters.");

            var exists = await _db.Characters
                .AnyAsync(c => c.PlayerId == playerId && c.Name == request.Name);
            if (exists)
                throw new ConflictException($"You already have a character named '{request.Name}'.");

            var baseStats = GetBaseStats(charClass);

            var character = new Character
            {
                Id = Guid.NewGuid(),
                PlayerId = playerId,
                Name = request.Name,
                Class = charClass,
                Level = 1,
                Experience = 0,
                Gold = 100,
                SkillPoints = 0,
                CreatedAt = DateTime.UtcNow
            };

            var stats = new CharacterStats
            {
                Id = Guid.NewGuid(),
                CharacterId = character.Id,
                HP = baseStats.HP,
                MaxHP = baseStats.HP,
                MP = baseStats.MP,
                MaxMP = baseStats.MP,
                Attack = baseStats.Attack,
                Defense = baseStats.Defense,
                MagicPower = baseStats.MagicPower,
                Speed = baseStats.Speed,
                CritChance = baseStats.CritChance,
                DodgeChance = baseStats.DodgeChance
            };

            _db.Characters.Add(character);
            _db.CharacterStats.Add(stats);

            var starterSkill = await _db.Skills
                .FirstOrDefaultAsync(s => s.Class == charClass && s.UnlockLevel == 1);
            if (starterSkill != null)
            {
                _db.LearnedSkills.Add(new LearnedSkill
                {
                    Id = Guid.NewGuid(),
                    CharacterId = character.Id,
                    SkillId = starterSkill.Id,
                    LearnedAt = DateTime.UtcNow
                });
            }

            await _db.SaveChangesAsync();

            return MapToResponse(character);
        }

        public async Task<List<CharacterResponse>> GetCharactersAsync(Guid playerId)
        {
            var characters = await _db.Characters
                .Where(c => c.PlayerId == playerId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return characters.Select(MapToResponse).ToList();
        }

        public async Task<CharacterStatsResponse> GetCharacterStatsAsync(Guid playerId, Guid characterId)
        {
            var character = await _db.Characters
                .Include(c => c.Stats)
                .Include(c => c.EquippedItems)
                    .ThenInclude(e => e.InventoryItem)
                        .ThenInclude(i => i.Item)
                .Include(c => c.LearnedSkills)
                    .ThenInclude(ls => ls.Skill)
                .FirstOrDefaultAsync(c => c.Id == characterId && c.PlayerId == playerId)
                ?? throw new NotFoundException($"Character {characterId} not found.");

            return MapToStatsResponse(character);
        }

        public async Task<CharacterStatsResponse> AllocateStatsAsync(Guid playerId, Guid characterId, AllocateStatsRequest request)
        {
            var character = await _db.Characters
                .Include(c => c.Stats)
                .Include(c => c.EquippedItems)
                    .ThenInclude(e => e.InventoryItem)
                        .ThenInclude(i => i.Item)
                .Include(c => c.LearnedSkills)
                    .ThenInclude(ls => ls.Skill)
                .FirstOrDefaultAsync(c => c.Id == characterId && c.PlayerId == playerId)
                ?? throw new NotFoundException($"Character {characterId} not found.");

            var totalPointsRequested = request.HP + request.MP + request.Attack + request.Defense + request.MagicPower + request.Speed;
            if (totalPointsRequested <= 0)
                throw new ValidationException("You must allocate at least 1 skill point.");
            if (totalPointsRequested > character.SkillPoints)
                throw new ValidationException($"Not enough skill points. Available: {character.SkillPoints}, requested: {totalPointsRequested}");
            if (request.HP < 0 || request.MP < 0 || request.Attack < 0 || request.Defense < 0 || request.MagicPower < 0 || request.Speed < 0)
                throw new ValidationException("Stat allocations cannot be negative.");

            character.Stats.MaxHP += request.HP * 5;
            character.Stats.HP += request.HP * 5;
            character.Stats.MaxMP += request.MP * 3;
            character.Stats.MP += request.MP * 3;
            character.Stats.Attack += request.Attack;
            character.Stats.Defense += request.Defense;
            character.Stats.MagicPower += request.MagicPower;
            character.Stats.Speed += request.Speed;

            character.SkillPoints -= totalPointsRequested;

            await _db.SaveChangesAsync();

            return MapToStatsResponse(character);
        }

        public async Task AwardExperienceAsync(Guid characterId, int xp)
        {
            var character = await _db.Characters
                .Include(c => c.Stats)
                .FirstOrDefaultAsync(c => c.Id == characterId)
                ?? throw new NotFoundException($"Character {characterId} not found.");

            character.Experience += xp;

            // Pre-load all learned skill IDs and class skills once to avoid N+1 in the loop
            var learnedSkillIds = (await _db.LearnedSkills
                .Where(ls => ls.CharacterId == character.Id)
                .Select(ls => ls.SkillId)
                .ToListAsync())
                .ToHashSet();

            var classSkills = await _db.Skills
                .Where(s => s.Class == character.Class)
                .ToListAsync();

            while (character.Level < 50)
            {
                var xpNeeded = GetXPForLevel(character.Level + 1);
                if (character.Experience < xpNeeded) break;

                character.Experience -= xpNeeded;
                character.Level++;
                character.SkillPoints += 3;

                ApplyLevelUpStats(character);

                // Unlock skills for the new level using in-memory data
                foreach (var skill in classSkills.Where(s => s.UnlockLevel <= character.Level && !learnedSkillIds.Contains(s.Id)))
                {
                    _db.LearnedSkills.Add(new LearnedSkill
                    {
                        Id = Guid.NewGuid(),
                        CharacterId = character.Id,
                        SkillId = skill.Id,
                        LearnedAt = DateTime.UtcNow
                    });
                    learnedSkillIds.Add(skill.Id);
                }
            }

            // Caller is responsible for SaveChangesAsync when called from CombatEngine;
            // standalone calls (e.g., quest rewards) should save explicitly.
        }

        public async Task AwardGoldAsync(Guid characterId, int gold)
        {
            var character = await _db.Characters
                .FirstOrDefaultAsync(c => c.Id == characterId)
                ?? throw new NotFoundException($"Character {characterId} not found.");

            character.Gold += gold;
            // Caller is responsible for SaveChangesAsync
        }

        /// <summary>
        /// XP curve: each level requires more XP. Formula: level^2 * 100
        /// </summary>
        public int GetXPForLevel(int level)
        {
            return level * level * 100;
        }

        #region Private Helpers

        private static BaseStatBlock GetBaseStats(CharacterClass charClass)
        {
            return charClass switch
            {
                CharacterClass.Warrior => new BaseStatBlock(150, 30, 18, 15, 5, 10, 8.0, 3.0),
                CharacterClass.Mage => new BaseStatBlock(80, 120, 8, 6, 22, 12, 10.0, 5.0),
                CharacterClass.Rogue => new BaseStatBlock(100, 50, 15, 8, 10, 20, 15.0, 12.0),
                CharacterClass.Paladin => new BaseStatBlock(130, 60, 14, 18, 12, 8, 6.0, 4.0),
                _ => throw new ValidationException("Unknown character class.")
            };
        }

        private static void ApplyLevelUpStats(Character character)
        {
            var scaling = character.Class switch
            {
                CharacterClass.Warrior => (hp: 12, mp: 2, atk: 3, def: 2, mag: 0, spd: 1),
                CharacterClass.Mage => (hp: 5, mp: 10, atk: 1, def: 1, mag: 4, spd: 1),
                CharacterClass.Rogue => (hp: 7, mp: 4, atk: 2, def: 1, mag: 1, spd: 3),
                CharacterClass.Paladin => (hp: 10, mp: 5, atk: 2, def: 3, mag: 2, spd: 1),
                _ => (hp: 8, mp: 4, atk: 2, def: 2, mag: 1, spd: 1)
            };

            character.Stats.MaxHP += scaling.hp;
            character.Stats.HP = character.Stats.MaxHP; // Full heal on level up
            character.Stats.MaxMP += scaling.mp;
            character.Stats.MP = character.Stats.MaxMP;
            character.Stats.Attack += scaling.atk;
            character.Stats.Defense += scaling.def;
            character.Stats.MagicPower += scaling.mag;
            character.Stats.Speed += scaling.spd;
        }

        private static CharacterResponse MapToResponse(Character c)
        {
            return new CharacterResponse(c.Id, c.Name, c.Class.ToString(), c.Level, c.Experience, c.Gold, c.SkillPoints, c.CreatedAt);
        }

        private static CharacterStatsResponse MapToStatsResponse(Character c)
        {
            var equipBonuses = EquipmentBonusCalculator.Calculate(c.EquippedItems);

            var equipment = c.EquippedItems.Select(e => new EquippedItemResponse(
                e.Slot, e.InventoryItem.Item.Name, e.InventoryItem.Item.Rarity.ToString(), e.InventoryItemId
            )).ToList();

            var skills = c.LearnedSkills.Select(ls => new LearnedSkillResponse(
                ls.SkillId, ls.Skill.Name, ls.Skill.Description,
                ls.Skill.Type.ToString(), ls.Skill.TargetType.ToString(),
                ls.Skill.ManaCost, ls.Skill.Cooldown
            )).ToList();

            return new CharacterStatsResponse(
                c.Id, c.Name, c.Class.ToString(), c.Level,
                c.Stats.HP + equipBonuses.HP,
                c.Stats.MaxHP + equipBonuses.HP,
                c.Stats.MP + equipBonuses.MP,
                c.Stats.MaxMP + equipBonuses.MP,
                c.Stats.Attack + equipBonuses.Attack,
                c.Stats.Defense + equipBonuses.Defense,
                c.Stats.MagicPower + equipBonuses.MagicPower,
                c.Stats.Speed + equipBonuses.Speed,
                c.Stats.CritChance + equipBonuses.CritChance,
                c.Stats.DodgeChance + equipBonuses.DodgeChance,
                equipment,
                skills
            );
        }

        #endregion

        private record BaseStatBlock(int HP, int MP, int Attack, int Defense, int MagicPower, int Speed, double CritChance, double DodgeChance);
    }
}
