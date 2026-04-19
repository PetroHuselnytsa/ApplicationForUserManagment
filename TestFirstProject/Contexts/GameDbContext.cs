using Microsoft.EntityFrameworkCore;
using TestFirstProject.Configurations.Game;
using TestFirstProject.Models.Game;

namespace TestFirstProject.Contexts
{
    /// <summary>
    /// DbContext for the RPG game engine. Separate from PersonsContext to keep
    /// the game domain isolated while sharing the same PostgreSQL database.
    /// </summary>
    public class GameDbContext : DbContext
    {
        // Character system
        public DbSet<Character> Characters { get; set; } = null!;
        public DbSet<CharacterStats> CharacterStats { get; set; } = null!;
        public DbSet<Skill> Skills { get; set; } = null!;
        public DbSet<LearnedSkill> LearnedSkills { get; set; } = null!;

        // Combat system
        public DbSet<Battle> Battles { get; set; } = null!;
        public DbSet<BattleParticipant> BattleParticipants { get; set; } = null!;
        public DbSet<ActiveStatusEffect> ActiveStatusEffects { get; set; } = null!;

        // Inventory system
        public DbSet<Item> Items { get; set; } = null!;
        public DbSet<InventoryItem> InventoryItems { get; set; } = null!;
        public DbSet<EquippedItem> EquippedItems { get; set; } = null!;
        public DbSet<Stash> Stashes { get; set; } = null!;

        // Enemy & loot system
        public DbSet<Enemy> Enemies { get; set; } = null!;
        public DbSet<LootTableEntry> LootTableEntries { get; set; } = null!;

        // Zone & dungeon system
        public DbSet<Zone> Zones { get; set; } = null!;
        public DbSet<DungeonRun> DungeonRuns { get; set; } = null!;
        public DbSet<DungeonRoom> DungeonRooms { get; set; } = null!;

        // Quest system
        public DbSet<Quest> Quests { get; set; } = null!;
        public DbSet<CharacterQuest> CharacterQuests { get; set; } = null!;

        public GameDbContext(DbContextOptions<GameDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Apply all game entity configurations
            modelBuilder.ApplyConfiguration(new CharacterConfiguration());
            modelBuilder.ApplyConfiguration(new CharacterStatsConfiguration());
            modelBuilder.ApplyConfiguration(new SkillConfiguration());
            modelBuilder.ApplyConfiguration(new LearnedSkillConfiguration());
            modelBuilder.ApplyConfiguration(new BattleConfiguration());
            modelBuilder.ApplyConfiguration(new BattleParticipantConfiguration());
            modelBuilder.ApplyConfiguration(new ActiveStatusEffectConfiguration());
            modelBuilder.ApplyConfiguration(new ItemConfiguration());
            modelBuilder.ApplyConfiguration(new InventoryItemConfiguration());
            modelBuilder.ApplyConfiguration(new EquippedItemConfiguration());
            modelBuilder.ApplyConfiguration(new StashConfiguration());
            modelBuilder.ApplyConfiguration(new EnemyConfiguration());
            modelBuilder.ApplyConfiguration(new LootTableEntryConfiguration());
            modelBuilder.ApplyConfiguration(new ZoneConfiguration());
            modelBuilder.ApplyConfiguration(new DungeonRunConfiguration());
            modelBuilder.ApplyConfiguration(new DungeonRoomConfiguration());
            modelBuilder.ApplyConfiguration(new QuestConfiguration());
            modelBuilder.ApplyConfiguration(new CharacterQuestConfiguration());

            // Apply seed data
            GameSeedData.Seed(modelBuilder);
        }
    }
}
