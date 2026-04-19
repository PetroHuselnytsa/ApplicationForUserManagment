using Microsoft.EntityFrameworkCore;
using TestFirstProject.Models.Game;
using TestFirstProject.Models.Game.Enums;

using GameConsumableType = TestFirstProject.Models.Game.GameConsumableType;

namespace TestFirstProject.Configurations.Game
{
    /// <summary>
    /// Seeds the database with initial game data: skills, items, enemies, zones, and quests.
    /// Uses deterministic GUIDs so migrations are repeatable.
    /// </summary>
    public static class GameSeedData
    {
        // --- Deterministic GUIDs for seed data ---

        // Zones
        private static readonly Guid Zone1Id = new("a0000001-0000-0000-0000-000000000001");
        private static readonly Guid Zone2Id = new("a0000001-0000-0000-0000-000000000002");
        private static readonly Guid Zone3Id = new("a0000001-0000-0000-0000-000000000003");

        // Enemies
        private static readonly Guid GoblinId = new("b0000001-0000-0000-0000-000000000001");
        private static readonly Guid SkeletonId = new("b0000001-0000-0000-0000-000000000002");
        private static readonly Guid WolfId = new("b0000001-0000-0000-0000-000000000003");
        private static readonly Guid OrcId = new("b0000001-0000-0000-0000-000000000004");
        private static readonly Guid DarkMageId = new("b0000001-0000-0000-0000-000000000005");
        private static readonly Guid StoneGolemId = new("b0000001-0000-0000-0000-000000000006");
        // Bosses
        private static readonly Guid GoblinKingId = new("b0000002-0000-0000-0000-000000000001");
        private static readonly Guid LichLordId = new("b0000002-0000-0000-0000-000000000002");
        private static readonly Guid DragonId = new("b0000002-0000-0000-0000-000000000003");

        // Items
        private static readonly Guid RustySwordId = new("c0000001-0000-0000-0000-000000000001");
        private static readonly Guid WoodenStaffId = new("c0000001-0000-0000-0000-000000000002");
        private static readonly Guid LeatherArmorId = new("c0000001-0000-0000-0000-000000000003");
        private static readonly Guid IronSwordId = new("c0000001-0000-0000-0000-000000000004");
        private static readonly Guid ArcaneStaffId = new("c0000001-0000-0000-0000-000000000005");
        private static readonly Guid SteelArmorId = new("c0000001-0000-0000-0000-000000000006");
        private static readonly Guid ShadowDaggerId = new("c0000001-0000-0000-0000-000000000007");
        private static readonly Guid HolyMaceId = new("c0000001-0000-0000-0000-000000000008");
        private static readonly Guid DragonSlayerId = new("c0000001-0000-0000-0000-000000000009");
        private static readonly Guid HealthPotionId = new("c0000002-0000-0000-0000-000000000001");
        private static readonly Guid ManaPotionId = new("c0000002-0000-0000-0000-000000000002");
        private static readonly Guid GreatHealthPotionId = new("c0000002-0000-0000-0000-000000000003");
        private static readonly Guid ScrollOfStrengthId = new("c0000002-0000-0000-0000-000000000004");
        private static readonly Guid ScrollOfHasteId = new("c0000002-0000-0000-0000-000000000005");
        private static readonly Guid EnchantmentStoneId = new("c0000003-0000-0000-0000-000000000001");
        private static readonly Guid MagicDustId = new("c0000003-0000-0000-0000-000000000002");
        private static readonly Guid AmuletOfPowerId = new("c0000001-0000-0000-0000-000000000010");

        // Skills (Warrior)
        private static readonly Guid WarriorSlashId = new("d0000001-0000-0000-0000-000000000001");
        private static readonly Guid WarriorShieldBashId = new("d0000001-0000-0000-0000-000000000002");
        private static readonly Guid WarriorWhirlwindId = new("d0000001-0000-0000-0000-000000000003");
        private static readonly Guid WarriorBerserkId = new("d0000001-0000-0000-0000-000000000004");
        private static readonly Guid WarriorToughnessId = new("d0000001-0000-0000-0000-000000000005");

        // Skills (Mage)
        private static readonly Guid MageFireballId = new("d0000002-0000-0000-0000-000000000001");
        private static readonly Guid MageIceShardId = new("d0000002-0000-0000-0000-000000000002");
        private static readonly Guid MageLightningBoltId = new("d0000002-0000-0000-0000-000000000003");
        private static readonly Guid MageManaShieldId = new("d0000002-0000-0000-0000-000000000004");
        private static readonly Guid MageArcaneIntellectId = new("d0000002-0000-0000-0000-000000000005");

        // Skills (Rogue)
        private static readonly Guid RogueBackstabId = new("d0000003-0000-0000-0000-000000000001");
        private static readonly Guid RoguePoisonBladeId = new("d0000003-0000-0000-0000-000000000002");
        private static readonly Guid RogueShadowStrikeId = new("d0000003-0000-0000-0000-000000000003");
        private static readonly Guid RogueEvasionId = new("d0000003-0000-0000-0000-000000000004");
        private static readonly Guid RogueBladeMasteryId = new("d0000003-0000-0000-0000-000000000005");

        // Skills (Paladin)
        private static readonly Guid PaladinSmiteId = new("d0000004-0000-0000-0000-000000000001");
        private static readonly Guid PaladinHolyLightId = new("d0000004-0000-0000-0000-000000000002");
        private static readonly Guid PaladinDivineShieldId = new("d0000004-0000-0000-0000-000000000003");
        private static readonly Guid PaladinConsecrationId = new("d0000004-0000-0000-0000-000000000004");
        private static readonly Guid PaladinDevotionId = new("d0000004-0000-0000-0000-000000000005");

        // Quests
        private static readonly Guid Quest1Id = new("e0000001-0000-0000-0000-000000000001");
        private static readonly Guid Quest2Id = new("e0000001-0000-0000-0000-000000000002");
        private static readonly Guid Quest3Id = new("e0000001-0000-0000-0000-000000000003");
        private static readonly Guid Quest4Id = new("e0000001-0000-0000-0000-000000000004");
        private static readonly Guid Quest5Id = new("e0000001-0000-0000-0000-000000000005");
        private static readonly Guid Quest6Id = new("e0000001-0000-0000-0000-000000000006");

        public static void Seed(ModelBuilder modelBuilder)
        {
            SeedZones(modelBuilder);
            SeedSkills(modelBuilder);
            SeedItems(modelBuilder);
            SeedEnemies(modelBuilder);
            SeedLootTable(modelBuilder);
            SeedQuests(modelBuilder);
        }

        private static void SeedZones(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Zone>().HasData(
                new Zone { Id = Zone1Id, Name = "Whispering Woods", Description = "A dark forest teeming with goblins and wolves. Ideal for new adventurers.", MinLevel = 1, MaxLevel = 10 },
                new Zone { Id = Zone2Id, Name = "Cursed Catacombs", Description = "Ancient underground tunnels haunted by the undead.", MinLevel = 10, MaxLevel = 25 },
                new Zone { Id = Zone3Id, Name = "Dragon's Peak", Description = "A volcanic mountain where only the strongest dare to venture.", MinLevel = 25, MaxLevel = 50 }
            );
        }

        private static void SeedSkills(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Skill>().HasData(
                // --- Warrior skills ---
                new Skill { Id = WarriorSlashId, Name = "Power Slash", Description = "A mighty sword strike that deals heavy physical damage.", Class = CharacterClass.Warrior, Type = SkillType.Active, UnlockLevel = 1, MpCost = 5, CooldownTurns = 0, TargetType = SkillTargetType.Single, DamageType = DamageType.Physical, BaseDamage = 15, DamageMultiplier = 1.5 },
                new Skill { Id = WarriorShieldBashId, Name = "Shield Bash", Description = "Bash the enemy with your shield, dealing damage and stunning them.", Class = CharacterClass.Warrior, Type = SkillType.Active, UnlockLevel = 5, MpCost = 12, CooldownTurns = 2, TargetType = SkillTargetType.Single, DamageType = DamageType.Physical, BaseDamage = 10, DamageMultiplier = 1.0, AppliesEffect = StatusEffectType.Stun, EffectDuration = 1, EffectTickValue = 0, EffectStackLimit = 1 },
                new Skill { Id = WarriorWhirlwindId, Name = "Whirlwind", Description = "Spin your weapon, striking all enemies.", Class = CharacterClass.Warrior, Type = SkillType.Active, UnlockLevel = 12, MpCost = 20, CooldownTurns = 3, TargetType = SkillTargetType.All, DamageType = DamageType.Physical, BaseDamage = 20, DamageMultiplier = 1.2 },
                new Skill { Id = WarriorBerserkId, Name = "Berserk", Description = "Enter a rage, applying Haste to yourself.", Class = CharacterClass.Warrior, Type = SkillType.Active, UnlockLevel = 20, MpCost = 15, CooldownTurns = 4, TargetType = SkillTargetType.Self, DamageType = DamageType.Physical, BaseDamage = 0, DamageMultiplier = 0, AppliesEffect = StatusEffectType.Haste, EffectDuration = 3, EffectTickValue = 5, EffectStackLimit = 1 },
                new Skill { Id = WarriorToughnessId, Name = "Toughness", Description = "Passive: Increases HP and Defense.", Class = CharacterClass.Warrior, Type = SkillType.Passive, UnlockLevel = 8, PassiveHpBonus = 50, PassiveDefenseBonus = 10 },

                // --- Mage skills ---
                new Skill { Id = MageFireballId, Name = "Fireball", Description = "Hurl a ball of fire that burns the target.", Class = CharacterClass.Mage, Type = SkillType.Active, UnlockLevel = 1, MpCost = 10, CooldownTurns = 0, TargetType = SkillTargetType.Single, DamageType = DamageType.Fire, BaseDamage = 20, DamageMultiplier = 1.8, AppliesEffect = StatusEffectType.Burn, EffectDuration = 3, EffectTickValue = 5, EffectStackLimit = 2 },
                new Skill { Id = MageIceShardId, Name = "Ice Shard", Description = "Launch a shard of ice that slows the target.", Class = CharacterClass.Mage, Type = SkillType.Active, UnlockLevel = 5, MpCost = 8, CooldownTurns = 1, TargetType = SkillTargetType.Single, DamageType = DamageType.Ice, BaseDamage = 15, DamageMultiplier = 1.5, AppliesEffect = StatusEffectType.Slow, EffectDuration = 2, EffectTickValue = 0, EffectStackLimit = 1 },
                new Skill { Id = MageLightningBoltId, Name = "Lightning Bolt", Description = "Call down lightning on all enemies.", Class = CharacterClass.Mage, Type = SkillType.Active, UnlockLevel = 15, MpCost = 25, CooldownTurns = 3, TargetType = SkillTargetType.All, DamageType = DamageType.Lightning, BaseDamage = 25, DamageMultiplier = 1.6 },
                new Skill { Id = MageManaShieldId, Name = "Mana Shield", Description = "Create a magical shield around yourself.", Class = CharacterClass.Mage, Type = SkillType.Active, UnlockLevel = 10, MpCost = 20, CooldownTurns = 4, TargetType = SkillTargetType.Self, DamageType = DamageType.Ice, BaseDamage = 0, DamageMultiplier = 0, AppliesEffect = StatusEffectType.Shield, EffectDuration = 3, EffectTickValue = 30, EffectStackLimit = 1 },
                new Skill { Id = MageArcaneIntellectId, Name = "Arcane Intellect", Description = "Passive: Increases MP and Magic Power.", Class = CharacterClass.Mage, Type = SkillType.Passive, UnlockLevel = 8, PassiveMpBonus = 50, PassiveMagicPowerBonus = 15 },

                // --- Rogue skills ---
                new Skill { Id = RogueBackstabId, Name = "Backstab", Description = "Strike from the shadows for massive damage.", Class = CharacterClass.Rogue, Type = SkillType.Active, UnlockLevel = 1, MpCost = 8, CooldownTurns = 0, TargetType = SkillTargetType.Single, DamageType = DamageType.Physical, BaseDamage = 25, DamageMultiplier = 2.0 },
                new Skill { Id = RoguePoisonBladeId, Name = "Poison Blade", Description = "Coat your blade in poison, dealing damage over time.", Class = CharacterClass.Rogue, Type = SkillType.Active, UnlockLevel = 5, MpCost = 10, CooldownTurns = 1, TargetType = SkillTargetType.Single, DamageType = DamageType.Poison, BaseDamage = 10, DamageMultiplier = 1.2, AppliesEffect = StatusEffectType.Poison, EffectDuration = 4, EffectTickValue = 8, EffectStackLimit = 3 },
                new Skill { Id = RogueShadowStrikeId, Name = "Shadow Strike", Description = "A devastating strike that causes bleeding.", Class = CharacterClass.Rogue, Type = SkillType.Active, UnlockLevel = 15, MpCost = 18, CooldownTurns = 2, TargetType = SkillTargetType.Single, DamageType = DamageType.Physical, BaseDamage = 30, DamageMultiplier = 1.8, AppliesEffect = StatusEffectType.Bleed, EffectDuration = 3, EffectTickValue = 6, EffectStackLimit = 2 },
                new Skill { Id = RogueEvasionId, Name = "Evasion", Description = "Passive: Increases Dodge Chance and Speed.", Class = CharacterClass.Rogue, Type = SkillType.Passive, UnlockLevel = 8, PassiveSpeedBonus = 10 },
                new Skill { Id = RogueBladeMasteryId, Name = "Blade Mastery", Description = "Passive: Increases Attack power.", Class = CharacterClass.Rogue, Type = SkillType.Passive, UnlockLevel = 20, PassiveAttackBonus = 20 },

                // --- Paladin skills ---
                new Skill { Id = PaladinSmiteId, Name = "Holy Smite", Description = "Strike with divine power.", Class = CharacterClass.Paladin, Type = SkillType.Active, UnlockLevel = 1, MpCost = 8, CooldownTurns = 0, TargetType = SkillTargetType.Single, DamageType = DamageType.Fire, BaseDamage = 18, DamageMultiplier = 1.4 },
                new Skill { Id = PaladinHolyLightId, Name = "Holy Light", Description = "Heal yourself with divine energy.", Class = CharacterClass.Paladin, Type = SkillType.Active, UnlockLevel = 5, MpCost = 15, CooldownTurns = 2, TargetType = SkillTargetType.Self, DamageType = DamageType.Fire, BaseDamage = 0, DamageMultiplier = 0, AppliesEffect = StatusEffectType.Regen, EffectDuration = 3, EffectTickValue = 15, EffectStackLimit = 1 },
                new Skill { Id = PaladinDivineShieldId, Name = "Divine Shield", Description = "Envelop yourself in a protective barrier.", Class = CharacterClass.Paladin, Type = SkillType.Active, UnlockLevel = 12, MpCost = 20, CooldownTurns = 4, TargetType = SkillTargetType.Self, DamageType = DamageType.Fire, BaseDamage = 0, DamageMultiplier = 0, AppliesEffect = StatusEffectType.Shield, EffectDuration = 2, EffectTickValue = 50, EffectStackLimit = 1 },
                new Skill { Id = PaladinConsecrationId, Name = "Consecration", Description = "Burn all enemies with holy fire.", Class = CharacterClass.Paladin, Type = SkillType.Active, UnlockLevel = 18, MpCost = 22, CooldownTurns = 3, TargetType = SkillTargetType.All, DamageType = DamageType.Fire, BaseDamage = 15, DamageMultiplier = 1.3, AppliesEffect = StatusEffectType.Burn, EffectDuration = 2, EffectTickValue = 4, EffectStackLimit = 1 },
                new Skill { Id = PaladinDevotionId, Name = "Devotion", Description = "Passive: Increases HP, Defense, and MP.", Class = CharacterClass.Paladin, Type = SkillType.Passive, UnlockLevel = 10, PassiveHpBonus = 30, PassiveDefenseBonus = 8, PassiveMpBonus = 20 }
            );
        }

        private static void SeedItems(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Item>().HasData(
                // --- Weapons ---
                new Item { Id = RustySwordId, Name = "Rusty Sword", Description = "A worn blade, but still sharp enough.", Type = ItemType.Weapon, Rarity = ItemRarity.Common, LevelRequirement = 1, BonusAttack = 5, BuyPrice = 50, SellPrice = 15 },
                new Item { Id = WoodenStaffId, Name = "Wooden Staff", Description = "A simple staff for channeling magic.", Type = ItemType.Weapon, Rarity = ItemRarity.Common, LevelRequirement = 1, ClassRestriction = CharacterClass.Mage, BonusMagicPower = 8, BuyPrice = 50, SellPrice = 15 },
                new Item { Id = IronSwordId, Name = "Iron Sword", Description = "A sturdy iron blade forged by a skilled smith.", Type = ItemType.Weapon, Rarity = ItemRarity.Uncommon, LevelRequirement = 5, BonusAttack = 12, BuyPrice = 200, SellPrice = 60 },
                new Item { Id = ArcaneStaffId, Name = "Arcane Staff", Description = "A staff imbued with arcane energy.", Type = ItemType.Weapon, Rarity = ItemRarity.Rare, LevelRequirement = 10, ClassRestriction = CharacterClass.Mage, BonusMagicPower = 25, BonusMp = 20, BuyPrice = 800, SellPrice = 250 },
                new Item { Id = ShadowDaggerId, Name = "Shadow Dagger", Description = "A dagger that strikes from the darkness.", Type = ItemType.Weapon, Rarity = ItemRarity.Rare, LevelRequirement = 12, ClassRestriction = CharacterClass.Rogue, BonusAttack = 18, BonusCritChance = 0.05, BuyPrice = 900, SellPrice = 280 },
                new Item { Id = HolyMaceId, Name = "Holy Mace", Description = "A mace blessed by the divine.", Type = ItemType.Weapon, Rarity = ItemRarity.Rare, LevelRequirement = 10, ClassRestriction = CharacterClass.Paladin, BonusAttack = 15, BonusMagicPower = 10, BuyPrice = 850, SellPrice = 260 },
                new Item { Id = DragonSlayerId, Name = "Dragonslayer", Description = "A legendary blade forged to slay dragons.", Type = ItemType.Weapon, Rarity = ItemRarity.Legendary, LevelRequirement = 30, ClassRestriction = CharacterClass.Warrior, BonusAttack = 50, BonusCritChance = 0.1, BuyPrice = 10000, SellPrice = 3000 },

                // --- Armor ---
                new Item { Id = LeatherArmorId, Name = "Leather Armor", Description = "Basic protection from light blows.", Type = ItemType.Armor, Rarity = ItemRarity.Common, LevelRequirement = 1, BonusDefense = 5, BonusHp = 10, BuyPrice = 60, SellPrice = 18 },
                new Item { Id = SteelArmorId, Name = "Steel Plate Armor", Description = "Heavy armor providing excellent defense.", Type = ItemType.Armor, Rarity = ItemRarity.Uncommon, LevelRequirement = 8, BonusDefense = 15, BonusHp = 30, BuyPrice = 400, SellPrice = 120 },

                // --- Accessory ---
                new Item { Id = AmuletOfPowerId, Name = "Amulet of Power", Description = "An amulet that radiates arcane energy.", Type = ItemType.Accessory, Rarity = ItemRarity.Epic, LevelRequirement = 15, BonusAttack = 8, BonusMagicPower = 12, BonusHp = 20, BuyPrice = 2000, SellPrice = 600 },

                // --- Consumables ---
                new Item { Id = HealthPotionId, Name = "Health Potion", Description = "Restores 50 HP.", Type = ItemType.Consumable, Rarity = ItemRarity.Common, ConsumableType = GameConsumableType.HealthPotion, HealAmount = 50, BuyPrice = 25, SellPrice = 8 },
                new Item { Id = ManaPotionId, Name = "Mana Potion", Description = "Restores 30 MP.", Type = ItemType.Consumable, Rarity = ItemRarity.Common, ConsumableType = GameConsumableType.ManaPotion, ManaAmount = 30, BuyPrice = 25, SellPrice = 8 },
                new Item { Id = GreatHealthPotionId, Name = "Great Health Potion", Description = "Restores 150 HP.", Type = ItemType.Consumable, Rarity = ItemRarity.Uncommon, ConsumableType = GameConsumableType.HealthPotion, HealAmount = 150, BuyPrice = 100, SellPrice = 30 },
                new Item { Id = ScrollOfStrengthId, Name = "Scroll of Strength", Description = "Temporarily increases Attack.", Type = ItemType.Consumable, Rarity = ItemRarity.Rare, ConsumableType = GameConsumableType.Scroll, BuffEffect = StatusEffectType.Haste, BuffDuration = 5, BuyPrice = 200, SellPrice = 60 },
                new Item { Id = ScrollOfHasteId, Name = "Scroll of Haste", Description = "Temporarily increases Speed.", Type = ItemType.Consumable, Rarity = ItemRarity.Rare, ConsumableType = GameConsumableType.Scroll, BuffEffect = StatusEffectType.Haste, BuffDuration = 5, BuyPrice = 200, SellPrice = 60 },

                // --- Materials ---
                new Item { Id = EnchantmentStoneId, Name = "Enchantment Stone", Description = "A crystallized stone used to enchant equipment.", Type = ItemType.Material, Rarity = ItemRarity.Uncommon, BuyPrice = 150, SellPrice = 50 },
                new Item { Id = MagicDustId, Name = "Magic Dust", Description = "Sparkling dust used in enchantments.", Type = ItemType.Material, Rarity = ItemRarity.Common, BuyPrice = 50, SellPrice = 15 }
            );
        }

        private static void SeedEnemies(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Enemy>().HasData(
                // Zone 1 — Whispering Woods
                new Enemy { Id = GoblinId, Name = "Goblin", ZoneId = Zone1Id, BaseHp = 60, BaseMp = 10, BaseAttack = 12, BaseDefense = 5, BaseMagicPower = 3, BaseSpeed = 8, BaseCritChance = 0.05, BaseDodgeChance = 0.08, PrimaryDamageType = DamageType.Physical, BaseXpReward = 20, BaseGoldReward = 10 },
                new Enemy { Id = WolfId, Name = "Dire Wolf", ZoneId = Zone1Id, BaseHp = 45, BaseMp = 0, BaseAttack = 15, BaseDefense = 3, BaseMagicPower = 0, BaseSpeed = 14, BaseCritChance = 0.1, BaseDodgeChance = 0.12, PrimaryDamageType = DamageType.Physical, BaseXpReward = 25, BaseGoldReward = 8 },
                new Enemy { Id = GoblinKingId, Name = "Goblin King", IsBoss = true, BossMechanic = BossMechanic.Summon, ZoneId = Zone1Id, BaseHp = 300, BaseMp = 50, BaseAttack = 25, BaseDefense = 15, BaseMagicPower = 10, BaseSpeed = 10, BaseCritChance = 0.08, BaseDodgeChance = 0.05, PrimaryDamageType = DamageType.Physical, BaseXpReward = 150, BaseGoldReward = 100 },

                // Zone 2 — Cursed Catacombs
                new Enemy { Id = SkeletonId, Name = "Skeleton Warrior", ZoneId = Zone2Id, BaseHp = 100, BaseMp = 0, BaseAttack = 22, BaseDefense = 18, BaseMagicPower = 0, BaseSpeed = 7, BaseCritChance = 0.06, BaseDodgeChance = 0.03, PrimaryDamageType = DamageType.Physical, BaseXpReward = 45, BaseGoldReward = 25 },
                new Enemy { Id = DarkMageId, Name = "Dark Mage", ZoneId = Zone2Id, BaseHp = 70, BaseMp = 80, BaseAttack = 8, BaseDefense = 10, BaseMagicPower = 30, BaseSpeed = 9, BaseCritChance = 0.07, BaseDodgeChance = 0.06, PrimaryDamageType = DamageType.Fire, BaseXpReward = 55, BaseGoldReward = 35 },
                new Enemy { Id = LichLordId, Name = "Lich Lord", IsBoss = true, BossMechanic = BossMechanic.ShieldPhase, ZoneId = Zone2Id, BaseHp = 600, BaseMp = 200, BaseAttack = 20, BaseDefense = 25, BaseMagicPower = 45, BaseSpeed = 8, BaseCritChance = 0.1, BaseDodgeChance = 0.05, PrimaryDamageType = DamageType.Ice, BaseXpReward = 400, BaseGoldReward = 300 },

                // Zone 3 — Dragon's Peak
                new Enemy { Id = OrcId, Name = "Orc Warlord", ZoneId = Zone3Id, BaseHp = 200, BaseMp = 20, BaseAttack = 40, BaseDefense = 30, BaseMagicPower = 5, BaseSpeed = 10, BaseCritChance = 0.12, BaseDodgeChance = 0.04, PrimaryDamageType = DamageType.Physical, BaseXpReward = 100, BaseGoldReward = 60 },
                new Enemy { Id = StoneGolemId, Name = "Stone Golem", ZoneId = Zone3Id, BaseHp = 350, BaseMp = 0, BaseAttack = 35, BaseDefense = 50, BaseMagicPower = 0, BaseSpeed = 4, BaseCritChance = 0.03, BaseDodgeChance = 0.0, PrimaryDamageType = DamageType.Physical, BaseXpReward = 120, BaseGoldReward = 70 },
                new Enemy { Id = DragonId, Name = "Ancient Dragon", IsBoss = true, BossMechanic = BossMechanic.Enrage, ZoneId = Zone3Id, BaseHp = 1500, BaseMp = 300, BaseAttack = 55, BaseDefense = 40, BaseMagicPower = 50, BaseSpeed = 12, BaseCritChance = 0.15, BaseDodgeChance = 0.08, PrimaryDamageType = DamageType.Fire, BaseXpReward = 1000, BaseGoldReward = 800 }
            );
        }

        private static void SeedLootTable(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<LootTableEntry>().HasData(
                // Goblin drops
                new LootTableEntry { Id = new Guid("f0000001-0000-0000-0000-000000000001"), EnemyId = GoblinId, ItemId = HealthPotionId, DropChance = 30.0, MinQuantity = 1, MaxQuantity = 2 },
                new LootTableEntry { Id = new Guid("f0000001-0000-0000-0000-000000000002"), EnemyId = GoblinId, ItemId = RustySwordId, DropChance = 10.0 },

                // Dire Wolf drops
                new LootTableEntry { Id = new Guid("f0000001-0000-0000-0000-000000000003"), EnemyId = WolfId, ItemId = LeatherArmorId, DropChance = 15.0 },
                new LootTableEntry { Id = new Guid("f0000001-0000-0000-0000-000000000004"), EnemyId = WolfId, ItemId = MagicDustId, DropChance = 25.0, MinQuantity = 1, MaxQuantity = 3 },

                // Goblin King drops
                new LootTableEntry { Id = new Guid("f0000001-0000-0000-0000-000000000005"), EnemyId = GoblinKingId, ItemId = IronSwordId, DropChance = 40.0 },
                new LootTableEntry { Id = new Guid("f0000001-0000-0000-0000-000000000006"), EnemyId = GoblinKingId, ItemId = EnchantmentStoneId, DropChance = 50.0, MinQuantity = 1, MaxQuantity = 2 },

                // Skeleton drops
                new LootTableEntry { Id = new Guid("f0000001-0000-0000-0000-000000000007"), EnemyId = SkeletonId, ItemId = SteelArmorId, DropChance = 8.0 },
                new LootTableEntry { Id = new Guid("f0000001-0000-0000-0000-000000000008"), EnemyId = SkeletonId, ItemId = EnchantmentStoneId, DropChance = 15.0 },

                // Dark Mage drops
                new LootTableEntry { Id = new Guid("f0000001-0000-0000-0000-000000000009"), EnemyId = DarkMageId, ItemId = ManaPotionId, DropChance = 35.0, MinQuantity = 1, MaxQuantity = 2 },
                new LootTableEntry { Id = new Guid("f0000001-0000-0000-0000-000000000010"), EnemyId = DarkMageId, ItemId = ArcaneStaffId, DropChance = 5.0 },

                // Lich Lord drops
                new LootTableEntry { Id = new Guid("f0000001-0000-0000-0000-000000000011"), EnemyId = LichLordId, ItemId = ArcaneStaffId, DropChance = 30.0 },
                new LootTableEntry { Id = new Guid("f0000001-0000-0000-0000-000000000012"), EnemyId = LichLordId, ItemId = AmuletOfPowerId, DropChance = 15.0 },

                // Orc Warlord drops
                new LootTableEntry { Id = new Guid("f0000001-0000-0000-0000-000000000013"), EnemyId = OrcId, ItemId = GreatHealthPotionId, DropChance = 25.0 },
                new LootTableEntry { Id = new Guid("f0000001-0000-0000-0000-000000000014"), EnemyId = OrcId, ItemId = ShadowDaggerId, DropChance = 8.0 },

                // Ancient Dragon drops
                new LootTableEntry { Id = new Guid("f0000001-0000-0000-0000-000000000015"), EnemyId = DragonId, ItemId = DragonSlayerId, DropChance = 10.0 },
                new LootTableEntry { Id = new Guid("f0000001-0000-0000-0000-000000000016"), EnemyId = DragonId, ItemId = ScrollOfStrengthId, DropChance = 40.0, MinQuantity = 1, MaxQuantity = 3 }
            );
        }

        private static void SeedQuests(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Quest>().HasData(
                // Zone 1 quest chain
                new Quest { Id = Quest1Id, Name = "Goblin Menace", Description = "Defeat 5 goblins threatening the village.", Type = QuestType.KillCount, RequiredCount = 5, TargetEnemyId = GoblinId, ZoneId = Zone1Id, XpReward = 100, GoldReward = 50, NextQuestId = Quest2Id, MinLevelRequirement = 1 },
                new Quest { Id = Quest2Id, Name = "The Goblin King", Description = "Slay the Goblin King lurking deep in the woods.", Type = QuestType.BossDefeat, RequiredCount = 1, TargetEnemyId = GoblinKingId, PrerequisiteQuestId = Quest1Id, ZoneId = Zone1Id, XpReward = 300, GoldReward = 150, RewardItemId = IronSwordId, MinLevelRequirement = 5 },

                // Zone 2 quest chain
                new Quest { Id = Quest3Id, Name = "Undead Rising", Description = "Defeat 10 skeletons in the catacombs.", Type = QuestType.KillCount, RequiredCount = 10, TargetEnemyId = SkeletonId, ZoneId = Zone2Id, XpReward = 250, GoldReward = 120, NextQuestId = Quest4Id, MinLevelRequirement = 10 },
                new Quest { Id = Quest4Id, Name = "Banish the Lich", Description = "Defeat the Lich Lord to cleanse the catacombs.", Type = QuestType.BossDefeat, RequiredCount = 1, TargetEnemyId = LichLordId, PrerequisiteQuestId = Quest3Id, ZoneId = Zone2Id, XpReward = 800, GoldReward = 400, RewardItemId = AmuletOfPowerId, MinLevelRequirement = 15 },

                // Zone 3 quest chain
                new Quest { Id = Quest5Id, Name = "Reach the Peak", Description = "Reach level 30 to prove your strength.", Type = QuestType.ReachLevel, TargetLevel = 30, ZoneId = Zone3Id, XpReward = 500, GoldReward = 300, NextQuestId = Quest6Id, MinLevelRequirement = 25 },
                new Quest { Id = Quest6Id, Name = "Slay the Dragon", Description = "Defeat the Ancient Dragon atop Dragon's Peak.", Type = QuestType.BossDefeat, RequiredCount = 1, TargetEnemyId = DragonId, PrerequisiteQuestId = Quest5Id, ZoneId = Zone3Id, XpReward = 2000, GoldReward = 1000, RewardItemId = DragonSlayerId, MinLevelRequirement = 30 }
            );
        }
    }
}
