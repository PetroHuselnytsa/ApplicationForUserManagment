using Microsoft.EntityFrameworkCore;
using TestFirstProject.Models.Enums;
using TestFirstProject.Models.Game;

namespace TestFirstProject.Data
{
    /// <summary>
    /// Seeds the database with initial RPG game data: skills, zones, enemies, bosses, items, loot tables, and quests.
    /// Called during migration or application startup.
    /// </summary>
    public static class GameSeedData
    {
        public static void SeedGameData(this ModelBuilder modelBuilder)
        {
            // ========================
            // ZONES
            // ========================
            var verdantForest = new Zone { Id = Guid.Parse("a0000001-0000-0000-0000-000000000001"), Name = "Verdant Forest", Description = "A lush forest teeming with wildlife and low-level creatures.", MinLevel = 1, MaxLevel = 10 };
            var darkCaverns = new Zone { Id = Guid.Parse("a0000001-0000-0000-0000-000000000002"), Name = "Dark Caverns", Description = "Deep underground caves filled with dangerous beasts and hidden treasures.", MinLevel = 10, MaxLevel = 20 };
            var volcanicWastes = new Zone { Id = Guid.Parse("a0000001-0000-0000-0000-000000000003"), Name = "Volcanic Wastes", Description = "A scorched landscape surrounding an active volcano. Only the brave venture here.", MinLevel = 20, MaxLevel = 35 };
            var shadowRealm = new Zone { Id = Guid.Parse("a0000001-0000-0000-0000-000000000004"), Name = "Shadow Realm", Description = "An otherworldly dimension where darkness reigns supreme.", MinLevel = 35, MaxLevel = 50 };

            modelBuilder.Entity<Zone>().HasData(verdantForest, darkCaverns, volcanicWastes, shadowRealm);

            // ========================
            // SKILLS - Warrior
            // ========================
            var warriorSkills = new[]
            {
                new Skill { Id = Guid.Parse("b0000001-0000-0000-0000-000000000001"), Name = "Power Strike", Description = "A mighty blow dealing increased physical damage.", Class = CharacterClass.Warrior, Type = SkillType.Active, DamageType = DamageType.Physical, TargetType = TargetType.Single, UnlockLevel = 1, ManaCost = 5, Cooldown = 0, DamageMultiplier = 1.5 },
                new Skill { Id = Guid.Parse("b0000001-0000-0000-0000-000000000002"), Name = "Whirlwind", Description = "Spin attack hitting all enemies.", Class = CharacterClass.Warrior, Type = SkillType.Active, DamageType = DamageType.Physical, TargetType = TargetType.All, UnlockLevel = 5, ManaCost = 12, Cooldown = 2, DamageMultiplier = 1.2 },
                new Skill { Id = Guid.Parse("b0000001-0000-0000-0000-000000000003"), Name = "Battle Cry", Description = "Boost your attack with a fierce war cry.", Class = CharacterClass.Warrior, Type = SkillType.Active, DamageType = DamageType.Physical, TargetType = TargetType.Self, UnlockLevel = 8, ManaCost = 8, Cooldown = 3, DamageMultiplier = 0, AppliesEffect = StatusEffectType.Haste, EffectDuration = 3, EffectTickValue = 0 },
                new Skill { Id = Guid.Parse("b0000001-0000-0000-0000-000000000004"), Name = "Rending Slash", Description = "A deep cut that causes bleeding.", Class = CharacterClass.Warrior, Type = SkillType.Active, DamageType = DamageType.Physical, TargetType = TargetType.Single, UnlockLevel = 12, ManaCost = 10, Cooldown = 1, DamageMultiplier = 1.3, AppliesEffect = StatusEffectType.Bleed, EffectDuration = 3, EffectTickValue = 5 },
                new Skill { Id = Guid.Parse("b0000001-0000-0000-0000-000000000005"), Name = "Iron Skin", Description = "Passively increases defense.", Class = CharacterClass.Warrior, Type = SkillType.Passive, DamageType = DamageType.Physical, TargetType = TargetType.Self, UnlockLevel = 15, PassiveBonusStat = "Defense", PassiveBonusValue = 10 },
                new Skill { Id = Guid.Parse("b0000001-0000-0000-0000-000000000006"), Name = "Earthquake", Description = "Slam the ground dealing massive AoE damage.", Class = CharacterClass.Warrior, Type = SkillType.Active, DamageType = DamageType.Physical, TargetType = TargetType.All, UnlockLevel = 25, ManaCost = 25, Cooldown = 3, DamageMultiplier = 2.0 },
            };

            // SKILLS - Mage
            var mageSkills = new[]
            {
                new Skill { Id = Guid.Parse("b0000002-0000-0000-0000-000000000001"), Name = "Fireball", Description = "Launch a ball of fire at an enemy.", Class = CharacterClass.Mage, Type = SkillType.Active, DamageType = DamageType.Fire, TargetType = TargetType.Single, UnlockLevel = 1, ManaCost = 8, Cooldown = 0, DamageMultiplier = 1.8, AppliesEffect = StatusEffectType.Burn, EffectDuration = 2, EffectTickValue = 4 },
                new Skill { Id = Guid.Parse("b0000002-0000-0000-0000-000000000002"), Name = "Ice Shard", Description = "Fire a shard of ice that slows the target.", Class = CharacterClass.Mage, Type = SkillType.Active, DamageType = DamageType.Ice, TargetType = TargetType.Single, UnlockLevel = 5, ManaCost = 10, Cooldown = 1, DamageMultiplier = 1.5, AppliesEffect = StatusEffectType.Slow, EffectDuration = 2, EffectTickValue = 0 },
                new Skill { Id = Guid.Parse("b0000002-0000-0000-0000-000000000003"), Name = "Lightning Bolt", Description = "Strike an enemy with lightning, chance to stun.", Class = CharacterClass.Mage, Type = SkillType.Active, DamageType = DamageType.Lightning, TargetType = TargetType.Single, UnlockLevel = 10, ManaCost = 15, Cooldown = 2, DamageMultiplier = 2.0, AppliesEffect = StatusEffectType.Stun, EffectDuration = 1, EffectTickValue = 0 },
                new Skill { Id = Guid.Parse("b0000002-0000-0000-0000-000000000004"), Name = "Arcane Shield", Description = "Surround yourself with a magical shield.", Class = CharacterClass.Mage, Type = SkillType.Active, DamageType = DamageType.Physical, TargetType = TargetType.Self, UnlockLevel = 8, ManaCost = 12, Cooldown = 3, DamageMultiplier = 0, AppliesEffect = StatusEffectType.Shield, EffectDuration = 3, EffectTickValue = 20 },
                new Skill { Id = Guid.Parse("b0000002-0000-0000-0000-000000000005"), Name = "Mana Flow", Description = "Passively increases max MP.", Class = CharacterClass.Mage, Type = SkillType.Passive, DamageType = DamageType.Physical, TargetType = TargetType.Self, UnlockLevel = 15, PassiveBonusStat = "MP", PassiveBonusValue = 30 },
                new Skill { Id = Guid.Parse("b0000002-0000-0000-0000-000000000006"), Name = "Meteor Storm", Description = "Call down meteors on all enemies.", Class = CharacterClass.Mage, Type = SkillType.Active, DamageType = DamageType.Fire, TargetType = TargetType.All, UnlockLevel = 25, ManaCost = 30, Cooldown = 4, DamageMultiplier = 2.5 },
            };

            // SKILLS - Rogue
            var rogueSkills = new[]
            {
                new Skill { Id = Guid.Parse("b0000003-0000-0000-0000-000000000001"), Name = "Backstab", Description = "Strike from behind for bonus damage.", Class = CharacterClass.Rogue, Type = SkillType.Active, DamageType = DamageType.Physical, TargetType = TargetType.Single, UnlockLevel = 1, ManaCost = 6, Cooldown = 0, DamageMultiplier = 1.6 },
                new Skill { Id = Guid.Parse("b0000003-0000-0000-0000-000000000002"), Name = "Poison Blade", Description = "Coat your blade in poison.", Class = CharacterClass.Rogue, Type = SkillType.Active, DamageType = DamageType.Poison, TargetType = TargetType.Single, UnlockLevel = 5, ManaCost = 8, Cooldown = 1, DamageMultiplier = 1.2, AppliesEffect = StatusEffectType.Poison, EffectDuration = 4, EffectTickValue = 3 },
                new Skill { Id = Guid.Parse("b0000003-0000-0000-0000-000000000003"), Name = "Shadow Step", Description = "Vanish and increase your speed.", Class = CharacterClass.Rogue, Type = SkillType.Active, DamageType = DamageType.Physical, TargetType = TargetType.Self, UnlockLevel = 8, ManaCost = 10, Cooldown = 3, DamageMultiplier = 0, AppliesEffect = StatusEffectType.Haste, EffectDuration = 3, EffectTickValue = 0 },
                new Skill { Id = Guid.Parse("b0000003-0000-0000-0000-000000000004"), Name = "Fan of Knives", Description = "Throw knives at all enemies.", Class = CharacterClass.Rogue, Type = SkillType.Active, DamageType = DamageType.Physical, TargetType = TargetType.All, UnlockLevel = 12, ManaCost = 15, Cooldown = 2, DamageMultiplier = 1.3 },
                new Skill { Id = Guid.Parse("b0000003-0000-0000-0000-000000000005"), Name = "Evasion Mastery", Description = "Passively increases dodge chance.", Class = CharacterClass.Rogue, Type = SkillType.Passive, DamageType = DamageType.Physical, TargetType = TargetType.Self, UnlockLevel = 15, PassiveBonusStat = "DodgeChance", PassiveBonusValue = 8 },
                new Skill { Id = Guid.Parse("b0000003-0000-0000-0000-000000000006"), Name = "Assassinate", Description = "A devastating finishing blow.", Class = CharacterClass.Rogue, Type = SkillType.Active, DamageType = DamageType.Physical, TargetType = TargetType.Single, UnlockLevel = 25, ManaCost = 20, Cooldown = 3, DamageMultiplier = 3.0 },
            };

            // SKILLS - Paladin
            var paladinSkills = new[]
            {
                new Skill { Id = Guid.Parse("b0000004-0000-0000-0000-000000000001"), Name = "Holy Strike", Description = "Smite an enemy with divine light.", Class = CharacterClass.Paladin, Type = SkillType.Active, DamageType = DamageType.Physical, TargetType = TargetType.Single, UnlockLevel = 1, ManaCost = 7, Cooldown = 0, DamageMultiplier = 1.4 },
                new Skill { Id = Guid.Parse("b0000004-0000-0000-0000-000000000002"), Name = "Divine Shield", Description = "Surround yourself with holy protection.", Class = CharacterClass.Paladin, Type = SkillType.Active, DamageType = DamageType.Physical, TargetType = TargetType.Self, UnlockLevel = 5, ManaCost = 10, Cooldown = 3, DamageMultiplier = 0, AppliesEffect = StatusEffectType.Shield, EffectDuration = 3, EffectTickValue = 25 },
                new Skill { Id = Guid.Parse("b0000004-0000-0000-0000-000000000003"), Name = "Healing Light", Description = "Heal yourself with divine energy.", Class = CharacterClass.Paladin, Type = SkillType.Active, DamageType = DamageType.Physical, TargetType = TargetType.Self, UnlockLevel = 8, ManaCost = 12, Cooldown = 2, DamageMultiplier = 0, AppliesEffect = StatusEffectType.Regen, EffectDuration = 3, EffectTickValue = 10 },
                new Skill { Id = Guid.Parse("b0000004-0000-0000-0000-000000000004"), Name = "Consecration", Description = "Consecrate the ground, burning all enemies.", Class = CharacterClass.Paladin, Type = SkillType.Active, DamageType = DamageType.Fire, TargetType = TargetType.All, UnlockLevel = 12, ManaCost = 15, Cooldown = 2, DamageMultiplier = 1.3, AppliesEffect = StatusEffectType.Burn, EffectDuration = 2, EffectTickValue = 4 },
                new Skill { Id = Guid.Parse("b0000004-0000-0000-0000-000000000005"), Name = "Righteous Aura", Description = "Passively increases HP.", Class = CharacterClass.Paladin, Type = SkillType.Passive, DamageType = DamageType.Physical, TargetType = TargetType.Self, UnlockLevel = 15, PassiveBonusStat = "HP", PassiveBonusValue = 50 },
                new Skill { Id = Guid.Parse("b0000004-0000-0000-0000-000000000006"), Name = "Judgment", Description = "Call down divine judgment on a single target.", Class = CharacterClass.Paladin, Type = SkillType.Active, DamageType = DamageType.Lightning, TargetType = TargetType.Single, UnlockLevel = 25, ManaCost = 22, Cooldown = 3, DamageMultiplier = 2.2 },
            };

            modelBuilder.Entity<Skill>().HasData(
                warriorSkills.Concat(mageSkills).Concat(rogueSkills).Concat(paladinSkills));

            // ========================
            // ITEMS
            // ========================
            var items = new[]
            {
                // Consumables
                new Item { Id = Guid.Parse("c0000001-0000-0000-0000-000000000001"), Name = "Minor Health Potion", Description = "Restores 50 HP.", Type = ItemType.Consumable, Rarity = ItemRarity.Common, ConsumableType = ConsumableType.HealthPotion, ConsumableValue = 50, GoldValue = 25 },
                new Item { Id = Guid.Parse("c0000001-0000-0000-0000-000000000002"), Name = "Health Potion", Description = "Restores 150 HP.", Type = ItemType.Consumable, Rarity = ItemRarity.Uncommon, ConsumableType = ConsumableType.HealthPotion, ConsumableValue = 150, GoldValue = 75 },
                new Item { Id = Guid.Parse("c0000001-0000-0000-0000-000000000003"), Name = "Greater Health Potion", Description = "Restores 350 HP.", Type = ItemType.Consumable, Rarity = ItemRarity.Rare, ConsumableType = ConsumableType.HealthPotion, ConsumableValue = 350, GoldValue = 200 },
                new Item { Id = Guid.Parse("c0000001-0000-0000-0000-000000000004"), Name = "Minor Mana Potion", Description = "Restores 30 MP.", Type = ItemType.Consumable, Rarity = ItemRarity.Common, ConsumableType = ConsumableType.ManaPotion, ConsumableValue = 30, GoldValue = 25 },
                new Item { Id = Guid.Parse("c0000001-0000-0000-0000-000000000005"), Name = "Mana Potion", Description = "Restores 80 MP.", Type = ItemType.Consumable, Rarity = ItemRarity.Uncommon, ConsumableType = ConsumableType.ManaPotion, ConsumableValue = 80, GoldValue = 75 },
                new Item { Id = Guid.Parse("c0000001-0000-0000-0000-000000000006"), Name = "Scroll of Haste", Description = "Temporarily boosts speed.", Type = ItemType.Consumable, Rarity = ItemRarity.Rare, ConsumableType = ConsumableType.Scroll, ConsumableValue = 0, ScrollEffect = StatusEffectType.Haste, ScrollEffectDuration = 3, GoldValue = 150 },
                new Item { Id = Guid.Parse("c0000001-0000-0000-0000-000000000007"), Name = "Scroll of Shield", Description = "Grants a temporary shield.", Type = ItemType.Consumable, Rarity = ItemRarity.Rare, ConsumableType = ConsumableType.Scroll, ConsumableValue = 30, ScrollEffect = StatusEffectType.Shield, ScrollEffectDuration = 3, GoldValue = 150 },

                // Weapons
                new Item { Id = Guid.Parse("c0000002-0000-0000-0000-000000000001"), Name = "Rusty Sword", Description = "A basic sword, slightly rusted.", Type = ItemType.Weapon, Rarity = ItemRarity.Common, LevelRequirement = 1, BonusAttack = 5, GoldValue = 50 },
                new Item { Id = Guid.Parse("c0000002-0000-0000-0000-000000000002"), Name = "Iron Sword", Description = "A sturdy iron sword.", Type = ItemType.Weapon, Rarity = ItemRarity.Uncommon, LevelRequirement = 5, BonusAttack = 12, GoldValue = 150 },
                new Item { Id = Guid.Parse("c0000002-0000-0000-0000-000000000003"), Name = "Steel Longsword", Description = "A well-crafted steel blade.", Type = ItemType.Weapon, Rarity = ItemRarity.Rare, LevelRequirement = 10, BonusAttack = 22, BonusCritChance = 3, GoldValue = 400 },
                new Item { Id = Guid.Parse("c0000002-0000-0000-0000-000000000004"), Name = "Arcane Staff", Description = "A staff infused with magical energy.", Type = ItemType.Weapon, Rarity = ItemRarity.Rare, LevelRequirement = 10, ClassRestriction = CharacterClass.Mage, BonusMagicPower = 25, BonusMP = 20, GoldValue = 450 },
                new Item { Id = Guid.Parse("c0000002-0000-0000-0000-000000000005"), Name = "Shadow Dagger", Description = "A dagger that strikes from the shadows.", Type = ItemType.Weapon, Rarity = ItemRarity.Rare, LevelRequirement = 10, ClassRestriction = CharacterClass.Rogue, BonusAttack = 18, BonusCritChance = 8, GoldValue = 420 },
                new Item { Id = Guid.Parse("c0000002-0000-0000-0000-000000000006"), Name = "Flamebrand", Description = "A legendary sword wreathed in fire.", Type = ItemType.Weapon, Rarity = ItemRarity.Epic, LevelRequirement = 20, BonusAttack = 35, BonusCritChance = 5, GoldValue = 1200 },
                new Item { Id = Guid.Parse("c0000002-0000-0000-0000-000000000007"), Name = "Excalibur", Description = "The legendary blade of kings.", Type = ItemType.Weapon, Rarity = ItemRarity.Legendary, LevelRequirement = 40, ClassRestriction = CharacterClass.Paladin, BonusAttack = 50, BonusMagicPower = 20, BonusCritChance = 10, GoldValue = 5000 },

                // Armor
                new Item { Id = Guid.Parse("c0000003-0000-0000-0000-000000000001"), Name = "Leather Vest", Description = "Basic leather protection.", Type = ItemType.Armor, Rarity = ItemRarity.Common, LevelRequirement = 1, BonusDefense = 5, BonusHP = 10, GoldValue = 40 },
                new Item { Id = Guid.Parse("c0000003-0000-0000-0000-000000000002"), Name = "Chain Mail", Description = "Linked metal rings for decent protection.", Type = ItemType.Armor, Rarity = ItemRarity.Uncommon, LevelRequirement = 5, BonusDefense = 12, BonusHP = 25, GoldValue = 175 },
                new Item { Id = Guid.Parse("c0000003-0000-0000-0000-000000000003"), Name = "Plate Armor", Description = "Heavy plate armor for maximum defense.", Type = ItemType.Armor, Rarity = ItemRarity.Rare, LevelRequirement = 15, ClassRestriction = CharacterClass.Warrior, BonusDefense = 25, BonusHP = 50, GoldValue = 600 },
                new Item { Id = Guid.Parse("c0000003-0000-0000-0000-000000000004"), Name = "Mage Robes", Description = "Enchanted robes boosting magic power.", Type = ItemType.Armor, Rarity = ItemRarity.Rare, LevelRequirement = 15, ClassRestriction = CharacterClass.Mage, BonusDefense = 8, BonusMagicPower = 15, BonusMP = 30, GoldValue = 550 },
                new Item { Id = Guid.Parse("c0000003-0000-0000-0000-000000000005"), Name = "Dragon Scale Armor", Description = "Armor forged from dragon scales.", Type = ItemType.Armor, Rarity = ItemRarity.Epic, LevelRequirement = 30, BonusDefense = 40, BonusHP = 80, GoldValue = 2000 },

                // Accessories
                new Item { Id = Guid.Parse("c0000004-0000-0000-0000-000000000001"), Name = "Silver Ring", Description = "A simple silver ring.", Type = ItemType.Accessory, Rarity = ItemRarity.Common, LevelRequirement = 1, BonusHP = 15, GoldValue = 30 },
                new Item { Id = Guid.Parse("c0000004-0000-0000-0000-000000000002"), Name = "Amulet of Speed", Description = "Increases the wearer's agility.", Type = ItemType.Accessory, Rarity = ItemRarity.Uncommon, LevelRequirement = 5, BonusSpeed = 5, BonusDodgeChance = 3, GoldValue = 200 },
                new Item { Id = Guid.Parse("c0000004-0000-0000-0000-000000000003"), Name = "Ring of Power", Description = "Amplifies physical and magical strength.", Type = ItemType.Accessory, Rarity = ItemRarity.Epic, LevelRequirement = 20, BonusAttack = 10, BonusMagicPower = 10, BonusCritChance = 5, GoldValue = 800 },

                // Materials (for enchanting)
                new Item { Id = Guid.Parse("c0000005-0000-0000-0000-000000000001"), Name = "Iron Ore", Description = "A chunk of raw iron.", Type = ItemType.Material, Rarity = ItemRarity.Common, GoldValue = 10 },
                new Item { Id = Guid.Parse("c0000005-0000-0000-0000-000000000002"), Name = "Magic Crystal", Description = "A crystal pulsing with magical energy.", Type = ItemType.Material, Rarity = ItemRarity.Uncommon, GoldValue = 40 },
                new Item { Id = Guid.Parse("c0000005-0000-0000-0000-000000000003"), Name = "Dragon Scale", Description = "A scale from a powerful dragon.", Type = ItemType.Material, Rarity = ItemRarity.Rare, GoldValue = 100 },
                new Item { Id = Guid.Parse("c0000005-0000-0000-0000-000000000004"), Name = "Phoenix Feather", Description = "A feather from a legendary phoenix.", Type = ItemType.Material, Rarity = ItemRarity.Epic, GoldValue = 300 },
                new Item { Id = Guid.Parse("c0000005-0000-0000-0000-000000000005"), Name = "Void Essence", Description = "Pure essence from the Shadow Realm.", Type = ItemType.Material, Rarity = ItemRarity.Legendary, GoldValue = 1000 },
            };
            modelBuilder.Entity<Item>().HasData(items);

            // ========================
            // ENEMIES - Verdant Forest (Zone 1)
            // ========================
            var enemies = new[]
            {
                new Enemy { Id = Guid.Parse("d0000001-0000-0000-0000-000000000001"), Name = "Forest Wolf", ZoneId = verdantForest.Id, IsBoss = false, DamageType = DamageType.Physical, BaseHP = 60, BaseMP = 0, BaseAttack = 12, BaseDefense = 5, BaseMagicPower = 0, BaseSpeed = 14, BaseCritChance = 5, BaseDodgeChance = 8, ExperienceReward = 30, GoldReward = 15, AIStrategy = "Default" },
                new Enemy { Id = Guid.Parse("d0000001-0000-0000-0000-000000000002"), Name = "Giant Spider", ZoneId = verdantForest.Id, IsBoss = false, DamageType = DamageType.Poison, BaseHP = 45, BaseMP = 10, BaseAttack = 10, BaseDefense = 3, BaseMagicPower = 8, BaseSpeed = 16, BaseCritChance = 10, BaseDodgeChance = 12, ExperienceReward = 25, GoldReward = 12, AIStrategy = "Aggressive" },
                new Enemy { Id = Guid.Parse("d0000001-0000-0000-0000-000000000003"), Name = "Treant", ZoneId = verdantForest.Id, IsBoss = false, DamageType = DamageType.Physical, BaseHP = 100, BaseMP = 0, BaseAttack = 8, BaseDefense = 12, BaseMagicPower = 0, BaseSpeed = 6, BaseCritChance = 2, BaseDodgeChance = 0, ExperienceReward = 35, GoldReward = 20, AIStrategy = "Defensive" },
                new Enemy { Id = Guid.Parse("d0000001-0000-0000-0000-000000000010"), Name = "Ancient Treant Lord", ZoneId = verdantForest.Id, IsBoss = true, BossMechanic = BossMechanic.Enrage, DamageType = DamageType.Physical, BaseHP = 300, BaseMP = 50, BaseAttack = 18, BaseDefense = 15, BaseMagicPower = 10, BaseSpeed = 8, BaseCritChance = 8, BaseDodgeChance = 3, ExperienceReward = 200, GoldReward = 100, AIStrategy = "Boss" },

                // Dark Caverns (Zone 2)
                new Enemy { Id = Guid.Parse("d0000002-0000-0000-0000-000000000001"), Name = "Cave Bat", ZoneId = darkCaverns.Id, IsBoss = false, DamageType = DamageType.Physical, BaseHP = 80, BaseMP = 0, BaseAttack = 18, BaseDefense = 8, BaseMagicPower = 0, BaseSpeed = 22, BaseCritChance = 12, BaseDodgeChance = 15, ExperienceReward = 60, GoldReward = 30, AIStrategy = "Aggressive" },
                new Enemy { Id = Guid.Parse("d0000002-0000-0000-0000-000000000002"), Name = "Stone Golem", ZoneId = darkCaverns.Id, IsBoss = false, DamageType = DamageType.Physical, BaseHP = 200, BaseMP = 0, BaseAttack = 22, BaseDefense = 25, BaseMagicPower = 0, BaseSpeed = 5, BaseCritChance = 3, BaseDodgeChance = 0, ExperienceReward = 80, GoldReward = 45, AIStrategy = "Defensive" },
                new Enemy { Id = Guid.Parse("d0000002-0000-0000-0000-000000000010"), Name = "Crystal Dragon", ZoneId = darkCaverns.Id, IsBoss = true, BossMechanic = BossMechanic.ShieldPhase, DamageType = DamageType.Ice, BaseHP = 600, BaseMP = 100, BaseAttack = 28, BaseDefense = 20, BaseMagicPower = 30, BaseSpeed = 12, BaseCritChance = 10, BaseDodgeChance = 5, ExperienceReward = 500, GoldReward = 250, AIStrategy = "Boss" },

                // Volcanic Wastes (Zone 3)
                new Enemy { Id = Guid.Parse("d0000003-0000-0000-0000-000000000001"), Name = "Fire Elemental", ZoneId = volcanicWastes.Id, IsBoss = false, DamageType = DamageType.Fire, BaseHP = 150, BaseMP = 50, BaseAttack = 25, BaseDefense = 12, BaseMagicPower = 30, BaseSpeed = 15, BaseCritChance = 8, BaseDodgeChance = 10, ExperienceReward = 120, GoldReward = 60, AIStrategy = "Aggressive" },
                new Enemy { Id = Guid.Parse("d0000003-0000-0000-0000-000000000002"), Name = "Lava Crawler", ZoneId = volcanicWastes.Id, IsBoss = false, DamageType = DamageType.Fire, BaseHP = 180, BaseMP = 0, BaseAttack = 30, BaseDefense = 18, BaseMagicPower = 0, BaseSpeed = 10, BaseCritChance = 5, BaseDodgeChance = 3, ExperienceReward = 130, GoldReward = 65, AIStrategy = "Default" },
                new Enemy { Id = Guid.Parse("d0000003-0000-0000-0000-000000000010"), Name = "Inferno Wyrm", ZoneId = volcanicWastes.Id, IsBoss = true, BossMechanic = BossMechanic.Summon, DamageType = DamageType.Fire, BaseHP = 1000, BaseMP = 150, BaseAttack = 40, BaseDefense = 25, BaseMagicPower = 45, BaseSpeed = 14, BaseCritChance = 12, BaseDodgeChance = 5, ExperienceReward = 1000, GoldReward = 500, AIStrategy = "Boss" },

                // Shadow Realm (Zone 4)
                new Enemy { Id = Guid.Parse("d0000004-0000-0000-0000-000000000001"), Name = "Shadow Wraith", ZoneId = shadowRealm.Id, IsBoss = false, DamageType = DamageType.Physical, BaseHP = 250, BaseMP = 80, BaseAttack = 35, BaseDefense = 15, BaseMagicPower = 40, BaseSpeed = 25, BaseCritChance = 15, BaseDodgeChance = 20, ExperienceReward = 250, GoldReward = 120, AIStrategy = "Aggressive" },
                new Enemy { Id = Guid.Parse("d0000004-0000-0000-0000-000000000002"), Name = "Void Sentinel", ZoneId = shadowRealm.Id, IsBoss = false, DamageType = DamageType.Lightning, BaseHP = 350, BaseMP = 60, BaseAttack = 40, BaseDefense = 30, BaseMagicPower = 35, BaseSpeed = 12, BaseCritChance = 8, BaseDodgeChance = 5, ExperienceReward = 300, GoldReward = 150, AIStrategy = "Defensive" },
                new Enemy { Id = Guid.Parse("d0000004-0000-0000-0000-000000000010"), Name = "The Void King", ZoneId = shadowRealm.Id, IsBoss = true, BossMechanic = BossMechanic.Enrage, DamageType = DamageType.Lightning, BaseHP = 2000, BaseMP = 300, BaseAttack = 55, BaseDefense = 35, BaseMagicPower = 60, BaseSpeed = 18, BaseCritChance = 15, BaseDodgeChance = 10, ExperienceReward = 3000, GoldReward = 1500, AIStrategy = "Boss" },
            };
            modelBuilder.Entity<Enemy>().HasData(enemies);

            // ========================
            // LOOT TABLES
            // ========================
            var lootEntries = new[]
            {
                // Forest Wolf
                new LootTableEntry { Id = Guid.Parse("e0000001-0000-0000-0000-000000000001"), EnemyId = Guid.Parse("d0000001-0000-0000-0000-000000000001"), ItemId = Guid.Parse("c0000001-0000-0000-0000-000000000001"), DropChance = 40, MinQuantity = 1, MaxQuantity = 2 },
                new LootTableEntry { Id = Guid.Parse("e0000001-0000-0000-0000-000000000002"), EnemyId = Guid.Parse("d0000001-0000-0000-0000-000000000001"), ItemId = Guid.Parse("c0000005-0000-0000-0000-000000000001"), DropChance = 30, MinQuantity = 1, MaxQuantity = 1 },
                // Giant Spider
                new LootTableEntry { Id = Guid.Parse("e0000001-0000-0000-0000-000000000003"), EnemyId = Guid.Parse("d0000001-0000-0000-0000-000000000002"), ItemId = Guid.Parse("c0000001-0000-0000-0000-000000000001"), DropChance = 50, MinQuantity = 1, MaxQuantity = 1 },
                // Treant
                new LootTableEntry { Id = Guid.Parse("e0000001-0000-0000-0000-000000000004"), EnemyId = Guid.Parse("d0000001-0000-0000-0000-000000000003"), ItemId = Guid.Parse("c0000005-0000-0000-0000-000000000001"), DropChance = 50, MinQuantity = 1, MaxQuantity = 3 },
                // Ancient Treant Lord (Boss)
                new LootTableEntry { Id = Guid.Parse("e0000001-0000-0000-0000-000000000005"), EnemyId = Guid.Parse("d0000001-0000-0000-0000-000000000010"), ItemId = Guid.Parse("c0000002-0000-0000-0000-000000000002"), DropChance = 60, MinQuantity = 1, MaxQuantity = 1 },
                new LootTableEntry { Id = Guid.Parse("e0000001-0000-0000-0000-000000000006"), EnemyId = Guid.Parse("d0000001-0000-0000-0000-000000000010"), ItemId = Guid.Parse("c0000003-0000-0000-0000-000000000002"), DropChance = 40, MinQuantity = 1, MaxQuantity = 1 },
                new LootTableEntry { Id = Guid.Parse("e0000001-0000-0000-0000-000000000007"), EnemyId = Guid.Parse("d0000001-0000-0000-0000-000000000010"), ItemId = Guid.Parse("c0000005-0000-0000-0000-000000000002"), DropChance = 80, MinQuantity = 1, MaxQuantity = 2 },
                // Crystal Dragon (Boss)
                new LootTableEntry { Id = Guid.Parse("e0000002-0000-0000-0000-000000000001"), EnemyId = Guid.Parse("d0000002-0000-0000-0000-000000000010"), ItemId = Guid.Parse("c0000002-0000-0000-0000-000000000003"), DropChance = 50, MinQuantity = 1, MaxQuantity = 1 },
                new LootTableEntry { Id = Guid.Parse("e0000002-0000-0000-0000-000000000002"), EnemyId = Guid.Parse("d0000002-0000-0000-0000-000000000010"), ItemId = Guid.Parse("c0000005-0000-0000-0000-000000000003"), DropChance = 70, MinQuantity = 1, MaxQuantity = 2 },
                // Inferno Wyrm (Boss)
                new LootTableEntry { Id = Guid.Parse("e0000003-0000-0000-0000-000000000001"), EnemyId = Guid.Parse("d0000003-0000-0000-0000-000000000010"), ItemId = Guid.Parse("c0000002-0000-0000-0000-000000000006"), DropChance = 30, MinQuantity = 1, MaxQuantity = 1 },
                new LootTableEntry { Id = Guid.Parse("e0000003-0000-0000-0000-000000000002"), EnemyId = Guid.Parse("d0000003-0000-0000-0000-000000000010"), ItemId = Guid.Parse("c0000003-0000-0000-0000-000000000005"), DropChance = 25, MinQuantity = 1, MaxQuantity = 1 },
                new LootTableEntry { Id = Guid.Parse("e0000003-0000-0000-0000-000000000003"), EnemyId = Guid.Parse("d0000003-0000-0000-0000-000000000010"), ItemId = Guid.Parse("c0000005-0000-0000-0000-000000000004"), DropChance = 60, MinQuantity = 1, MaxQuantity = 3 },
                // The Void King (Boss)
                new LootTableEntry { Id = Guid.Parse("e0000004-0000-0000-0000-000000000001"), EnemyId = Guid.Parse("d0000004-0000-0000-0000-000000000010"), ItemId = Guid.Parse("c0000002-0000-0000-0000-000000000007"), DropChance = 10, MinQuantity = 1, MaxQuantity = 1 },
                new LootTableEntry { Id = Guid.Parse("e0000004-0000-0000-0000-000000000002"), EnemyId = Guid.Parse("d0000004-0000-0000-0000-000000000010"), ItemId = Guid.Parse("c0000004-0000-0000-0000-000000000003"), DropChance = 40, MinQuantity = 1, MaxQuantity = 1 },
                new LootTableEntry { Id = Guid.Parse("e0000004-0000-0000-0000-000000000003"), EnemyId = Guid.Parse("d0000004-0000-0000-0000-000000000010"), ItemId = Guid.Parse("c0000005-0000-0000-0000-000000000005"), DropChance = 50, MinQuantity = 1, MaxQuantity = 2 },
            };
            modelBuilder.Entity<LootTableEntry>().HasData(lootEntries);

            // ========================
            // QUESTS
            // ========================
            var quests = new[]
            {
                // Zone 1 quests
                new Quest { Id = Guid.Parse("f0000001-0000-0000-0000-000000000001"), Name = "Wolf Slayer", Description = "Hunt 5 Forest Wolves to protect the village.", Type = QuestType.KillCount, ZoneId = verdantForest.Id, RequiredCount = 5, TargetName = "Forest Wolf", RewardXP = 150, RewardGold = 75, MinLevel = 1 },
                new Quest { Id = Guid.Parse("f0000001-0000-0000-0000-000000000002"), Name = "Spider Menace", Description = "Eliminate 3 Giant Spiders lurking in the forest.", Type = QuestType.KillCount, ZoneId = verdantForest.Id, RequiredCount = 3, TargetName = "Giant Spider", RewardXP = 100, RewardGold = 50, MinLevel = 2, PrerequisiteQuestId = Guid.Parse("f0000001-0000-0000-0000-000000000001") },
                new Quest { Id = Guid.Parse("f0000001-0000-0000-0000-000000000003"), Name = "The Ancient Guardian", Description = "Defeat the Ancient Treant Lord deep in the forest.", Type = QuestType.BossDefeat, ZoneId = verdantForest.Id, RequiredCount = 1, TargetName = "Ancient Treant Lord", RewardXP = 500, RewardGold = 200, RewardItemId = Guid.Parse("c0000002-0000-0000-0000-000000000002"), MinLevel = 5, PrerequisiteQuestId = Guid.Parse("f0000001-0000-0000-0000-000000000002") },

                // Zone 2 quests
                new Quest { Id = Guid.Parse("f0000002-0000-0000-0000-000000000001"), Name = "Cave Explorer", Description = "Reach level 12 to prove your readiness for the caverns.", Type = QuestType.ReachLevel, ZoneId = darkCaverns.Id, RequiredCount = 1, TargetLevel = 12, RewardXP = 300, RewardGold = 150, MinLevel = 10 },
                new Quest { Id = Guid.Parse("f0000002-0000-0000-0000-000000000002"), Name = "Crystal Collector", Description = "Collect 3 Magic Crystals from the cavern depths.", Type = QuestType.CollectItem, ZoneId = darkCaverns.Id, RequiredCount = 3, TargetItemId = Guid.Parse("c0000005-0000-0000-0000-000000000002"), RewardXP = 400, RewardGold = 200, MinLevel = 12, PrerequisiteQuestId = Guid.Parse("f0000002-0000-0000-0000-000000000001") },
                new Quest { Id = Guid.Parse("f0000002-0000-0000-0000-000000000003"), Name = "Dragon Slayer", Description = "Defeat the Crystal Dragon.", Type = QuestType.BossDefeat, ZoneId = darkCaverns.Id, RequiredCount = 1, TargetName = "Crystal Dragon", RewardXP = 1000, RewardGold = 500, RewardItemId = Guid.Parse("c0000002-0000-0000-0000-000000000004"), MinLevel = 15, PrerequisiteQuestId = Guid.Parse("f0000002-0000-0000-0000-000000000002") },

                // Zone 3 quests
                new Quest { Id = Guid.Parse("f0000003-0000-0000-0000-000000000001"), Name = "Fire Extinguisher", Description = "Defeat 10 Fire Elementals in the Volcanic Wastes.", Type = QuestType.KillCount, ZoneId = volcanicWastes.Id, RequiredCount = 10, TargetName = "Fire Elemental", RewardXP = 800, RewardGold = 400, MinLevel = 20 },
                new Quest { Id = Guid.Parse("f0000003-0000-0000-0000-000000000002"), Name = "Wyrm Hunter", Description = "Defeat the Inferno Wyrm. Time limit: 60 minutes.", Type = QuestType.BossDefeat, ZoneId = volcanicWastes.Id, RequiredCount = 1, TargetName = "Inferno Wyrm", RewardXP = 2500, RewardGold = 1000, RewardItemId = Guid.Parse("c0000002-0000-0000-0000-000000000006"), MinLevel = 25, TimeLimitMinutes = 60, PrerequisiteQuestId = Guid.Parse("f0000003-0000-0000-0000-000000000001") },

                // Zone 4 quests
                new Quest { Id = Guid.Parse("f0000004-0000-0000-0000-000000000001"), Name = "Shadow Walker", Description = "Reach level 40 to unlock the final challenge.", Type = QuestType.ReachLevel, ZoneId = shadowRealm.Id, RequiredCount = 1, TargetLevel = 40, RewardXP = 2000, RewardGold = 1000, MinLevel = 35 },
                new Quest { Id = Guid.Parse("f0000004-0000-0000-0000-000000000002"), Name = "Dethrone the Void King", Description = "Defeat The Void King and save the realm.", Type = QuestType.BossDefeat, ZoneId = shadowRealm.Id, RequiredCount = 1, TargetName = "The Void King", RewardXP = 10000, RewardGold = 5000, RewardItemId = Guid.Parse("c0000002-0000-0000-0000-000000000007"), MinLevel = 40, PrerequisiteQuestId = Guid.Parse("f0000004-0000-0000-0000-000000000001") },
            };
            modelBuilder.Entity<Quest>().HasData(quests);
        }
    }
}
