using Microsoft.EntityFrameworkCore;
using TestFirstProject.Contexts;
using TestFirstProject.Exceptions;
using TestFirstProject.Models.Game;
using TestFirstProject.Models.Game.Enums;
using TestFirstProject.Services.Interfaces.Game;

namespace TestFirstProject.Services.Implementations.Game
{
    /// <summary>
    /// Handles character creation, XP-based leveling (1-50), stat allocation,
    /// and skill unlocks per class.
    /// </summary>
    public class CharacterProgressionService : ICharacterProgressionService
    {
        private readonly GameDbContext _db;

        public CharacterProgressionService(GameDbContext db)
        {
            _db = db;
        }

        public async Task<Character> CreateCharacterAsync(Guid playerId, string name, CharacterClass characterClass)
        {
            if (string.IsNullOrWhiteSpace(name) || name.Length > 50)
                throw new ValidationException("Character name must be between 1 and 50 characters.");

            // Check for duplicate name per player
            var exists = await _db.Characters.AnyAsync(c => c.PlayerId == playerId && c.Name == name);
            if (exists)
                throw new ConflictException($"You already have a character named '{name}'.");

            var baseStats = CalculateBaseStats(characterClass, 1);

            var character = new Character
            {
                Id = Guid.NewGuid(),
                PlayerId = playerId,
                Name = name,
                Class = characterClass,
                Level = 1,
                Experience = 0,
                SkillPoints = 0,
                Gold = 100 // Starting gold
            };

            // Set initial HP/MP to max
            character.CurrentHp = baseStats.BaseHp;
            character.CurrentMp = baseStats.BaseMp;

            _db.Characters.Add(character);

            // Create stats record
            baseStats.Id = Guid.NewGuid();
            baseStats.CharacterId = character.Id;
            _db.CharacterStats.Add(baseStats);

            // Auto-learn level 1 skills
            var starterSkills = await _db.Skills
                .Where(s => s.Class == characterClass && s.UnlockLevel <= 1)
                .ToListAsync();

            foreach (var skill in starterSkills)
            {
                _db.LearnedSkills.Add(new LearnedSkill
                {
                    Id = Guid.NewGuid(),
                    CharacterId = character.Id,
                    SkillId = skill.Id
                });
            }

            await _db.SaveChangesAsync();

            // Reload with navigation properties
            return await GetCharacterAsync(character.Id, playerId);
        }

        public async Task<List<Character>> GetPlayerCharactersAsync(Guid playerId)
        {
            return await _db.Characters
                .Include(c => c.Stats)
                .Where(c => c.PlayerId == playerId)
                .OrderBy(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<Character> GetCharacterAsync(Guid characterId, Guid playerId)
        {
            var character = await _db.Characters
                .Include(c => c.Stats)
                .Include(c => c.LearnedSkills).ThenInclude(ls => ls.Skill)
                .Include(c => c.EquippedItems).ThenInclude(ei => ei.InventoryItem).ThenInclude(ii => ii.Item)
                .FirstOrDefaultAsync(c => c.Id == characterId && c.PlayerId == playerId);

            if (character == null)
                throw new NotFoundException("Character not found.");

            return character;
        }

        public async Task<Character> AwardExperienceAsync(Guid characterId, int xp)
        {
            var character = await _db.Characters
                .Include(c => c.Stats)
                .FirstOrDefaultAsync(c => c.Id == characterId);

            if (character == null)
                throw new NotFoundException("Character not found.");

            character.Experience += xp;

            // Check for level ups (cap at 50)
            while (character.Level < 50)
            {
                var xpNeeded = GetXpForLevel(character.Level + 1);
                if (character.Experience < xpNeeded) break;

                character.Experience -= xpNeeded;
                character.Level++;
                character.SkillPoints += 3; // 3 skill points per level

                // Recalculate base stats for new level
                var newStats = CalculateBaseStats(character.Class, character.Level);
                character.Stats.BaseHp = newStats.BaseHp;
                character.Stats.BaseMp = newStats.BaseMp;
                character.Stats.BaseAttack = newStats.BaseAttack;
                character.Stats.BaseDefense = newStats.BaseDefense;
                character.Stats.BaseMagicPower = newStats.BaseMagicPower;
                character.Stats.BaseSpeed = newStats.BaseSpeed;
                character.Stats.BaseCritChance = newStats.BaseCritChance;
                character.Stats.BaseDodgeChance = newStats.BaseDodgeChance;

                // Restore HP/MP on level up
                character.CurrentHp = character.Stats.TotalHp;
                character.CurrentMp = character.Stats.TotalMp;

                // Auto-learn skills at new level
                var newSkills = await _db.Skills
                    .Where(s => s.Class == character.Class && s.UnlockLevel == character.Level)
                    .ToListAsync();

                foreach (var skill in newSkills)
                {
                    var alreadyLearned = await _db.LearnedSkills
                        .AnyAsync(ls => ls.CharacterId == characterId && ls.SkillId == skill.Id);

                    if (!alreadyLearned)
                    {
                        _db.LearnedSkills.Add(new LearnedSkill
                        {
                            Id = Guid.NewGuid(),
                            CharacterId = characterId,
                            SkillId = skill.Id
                        });
                    }
                }
            }

            await _db.SaveChangesAsync();
            return character;
        }

        public async Task<CharacterStats> AllocateStatsAsync(Guid characterId, Guid playerId, int hp, int mp, int attack, int defense, int magicPower, int speed)
        {
            var totalPoints = hp + mp + attack + defense + magicPower + speed;
            if (totalPoints <= 0)
                throw new ValidationException("Must allocate at least 1 skill point.");

            var character = await _db.Characters
                .Include(c => c.Stats)
                .FirstOrDefaultAsync(c => c.Id == characterId && c.PlayerId == playerId);

            if (character == null)
                throw new NotFoundException("Character not found.");

            if (totalPoints > character.SkillPoints)
                throw new ValidationException($"Not enough skill points. Available: {character.SkillPoints}, Requested: {totalPoints}");

            if (hp < 0 || mp < 0 || attack < 0 || defense < 0 || magicPower < 0 || speed < 0)
                throw new ValidationException("Cannot allocate negative points.");

            character.SkillPoints -= totalPoints;
            character.Stats.BonusHp += hp * 5;       // Each point = 5 HP
            character.Stats.BonusMp += mp * 3;        // Each point = 3 MP
            character.Stats.BonusAttack += attack * 2; // Each point = 2 Attack
            character.Stats.BonusDefense += defense * 2;
            character.Stats.BonusMagicPower += magicPower * 2;
            character.Stats.BonusSpeed += speed * 1;

            await _db.SaveChangesAsync();
            return character.Stats;
        }

        public CharacterStats CalculateBaseStats(CharacterClass characterClass, int level)
        {
            // Base stats at level 1, with per-level scaling per class
            var stats = new CharacterStats();

            switch (characterClass)
            {
                case CharacterClass.Warrior:
                    stats.BaseHp = 120 + (level - 1) * 15;
                    stats.BaseMp = 30 + (level - 1) * 3;
                    stats.BaseAttack = 15 + (level - 1) * 3;
                    stats.BaseDefense = 12 + (level - 1) * 3;
                    stats.BaseMagicPower = 3 + (level - 1) * 1;
                    stats.BaseSpeed = 8 + (level - 1) * 1;
                    stats.BaseCritChance = 0.05 + (level - 1) * 0.002;
                    stats.BaseDodgeChance = 0.03 + (level - 1) * 0.001;
                    break;

                case CharacterClass.Mage:
                    stats.BaseHp = 70 + (level - 1) * 8;
                    stats.BaseMp = 100 + (level - 1) * 10;
                    stats.BaseAttack = 5 + (level - 1) * 1;
                    stats.BaseDefense = 5 + (level - 1) * 2;
                    stats.BaseMagicPower = 18 + (level - 1) * 4;
                    stats.BaseSpeed = 7 + (level - 1) * 1;
                    stats.BaseCritChance = 0.08 + (level - 1) * 0.003;
                    stats.BaseDodgeChance = 0.04 + (level - 1) * 0.001;
                    break;

                case CharacterClass.Rogue:
                    stats.BaseHp = 80 + (level - 1) * 9;
                    stats.BaseMp = 50 + (level - 1) * 5;
                    stats.BaseAttack = 14 + (level - 1) * 3;
                    stats.BaseDefense = 7 + (level - 1) * 2;
                    stats.BaseMagicPower = 5 + (level - 1) * 1;
                    stats.BaseSpeed = 14 + (level - 1) * 2;
                    stats.BaseCritChance = 0.12 + (level - 1) * 0.004;
                    stats.BaseDodgeChance = 0.10 + (level - 1) * 0.003;
                    break;

                case CharacterClass.Paladin:
                    stats.BaseHp = 100 + (level - 1) * 12;
                    stats.BaseMp = 60 + (level - 1) * 6;
                    stats.BaseAttack = 12 + (level - 1) * 2;
                    stats.BaseDefense = 14 + (level - 1) * 3;
                    stats.BaseMagicPower = 10 + (level - 1) * 2;
                    stats.BaseSpeed = 7 + (level - 1) * 1;
                    stats.BaseCritChance = 0.05 + (level - 1) * 0.002;
                    stats.BaseDodgeChance = 0.03 + (level - 1) * 0.001;
                    break;
            }

            return stats;
        }

        public int GetXpForLevel(int level)
        {
            // XP curve: each level requires more XP (quadratic)
            return 50 + (level * level * 10);
        }

        public async Task<CharacterStats> GetTotalStatsAsync(Guid characterId)
        {
            var character = await _db.Characters
                .Include(c => c.Stats)
                .Include(c => c.EquippedItems).ThenInclude(ei => ei.InventoryItem).ThenInclude(ii => ii.Item)
                .Include(c => c.LearnedSkills).ThenInclude(ls => ls.Skill)
                .FirstOrDefaultAsync(c => c.Id == characterId);

            if (character == null)
                throw new NotFoundException("Character not found.");

            // Start with base + bonus stats
            var totalStats = new CharacterStats
            {
                CharacterId = characterId,
                BaseHp = character.Stats.BaseHp + character.Stats.BonusHp,
                BaseMp = character.Stats.BaseMp + character.Stats.BonusMp,
                BaseAttack = character.Stats.BaseAttack + character.Stats.BonusAttack,
                BaseDefense = character.Stats.BaseDefense + character.Stats.BonusDefense,
                BaseMagicPower = character.Stats.BaseMagicPower + character.Stats.BonusMagicPower,
                BaseSpeed = character.Stats.BaseSpeed + character.Stats.BonusSpeed,
                BaseCritChance = character.Stats.BaseCritChance,
                BaseDodgeChance = character.Stats.BaseDodgeChance
            };

            // Add equipment bonuses
            foreach (var equipped in character.EquippedItems)
            {
                var item = equipped.InventoryItem.Item;
                var inv = equipped.InventoryItem;
                totalStats.BaseHp += item.BonusHp + inv.EnchantBonusHp;
                totalStats.BaseMp += item.BonusMp;
                totalStats.BaseAttack += item.BonusAttack + inv.EnchantBonusAttack;
                totalStats.BaseDefense += item.BonusDefense + inv.EnchantBonusDefense;
                totalStats.BaseMagicPower += item.BonusMagicPower + inv.EnchantBonusMagicPower;
                totalStats.BaseSpeed += item.BonusSpeed;
                totalStats.BaseCritChance += item.BonusCritChance;
                totalStats.BaseDodgeChance += item.BonusDodgeChance;
            }

            // Add passive skill bonuses
            foreach (var ls in character.LearnedSkills.Where(ls => ls.Skill.Type == SkillType.Passive))
            {
                totalStats.BaseHp += ls.Skill.PassiveHpBonus;
                totalStats.BaseMp += ls.Skill.PassiveMpBonus;
                totalStats.BaseAttack += ls.Skill.PassiveAttackBonus;
                totalStats.BaseDefense += ls.Skill.PassiveDefenseBonus;
                totalStats.BaseMagicPower += ls.Skill.PassiveMagicPowerBonus;
                totalStats.BaseSpeed += ls.Skill.PassiveSpeedBonus;
            }

            return totalStats;
        }
    }
}
