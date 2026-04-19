using Microsoft.EntityFrameworkCore;
using TestFirstProject.Configurations;
using TestFirstProject.Configurations.Game;
using TestFirstProject.Data;
using TestFirstProject.Models;
using TestFirstProject.Models.Game;

namespace TestFirstProject.Contexts
{
    public class PersonsContext : DbContext
    {
        public DbSet<Person> Persons { get; set; } = null!;

        // Messaging & notification entities
        public DbSet<AppUser> AppUsers { get; set; } = null!;
        public DbSet<Conversation> Conversations { get; set; } = null!;
        public DbSet<ConversationParticipant> ConversationParticipants { get; set; } = null!;
        public DbSet<Message> Messages { get; set; } = null!;
        public DbSet<Notification> Notifications { get; set; } = null!;
        public DbSet<NotificationPreference> NotificationPreferences { get; set; } = null!;

        // RPG Game Engine entities
        public DbSet<Character> Characters { get; set; } = null!;
        public DbSet<CharacterStats> CharacterStats { get; set; } = null!;
        public DbSet<Skill> Skills { get; set; } = null!;
        public DbSet<LearnedSkill> LearnedSkills { get; set; } = null!;
        public DbSet<Battle> Battles { get; set; } = null!;
        public DbSet<BattleParticipant> BattleParticipants { get; set; } = null!;
        public DbSet<BattleTurnLog> BattleTurnLogs { get; set; } = null!;
        public DbSet<ActiveStatusEffect> ActiveStatusEffects { get; set; } = null!;
        public DbSet<Enemy> Enemies { get; set; } = null!;
        public DbSet<EnemySkill> EnemySkills { get; set; } = null!;
        public DbSet<LootTableEntry> LootTableEntries { get; set; } = null!;
        public DbSet<Item> Items { get; set; } = null!;
        public DbSet<InventoryItem> InventoryItems { get; set; } = null!;
        public DbSet<EquippedItem> EquippedItems { get; set; } = null!;
        public DbSet<Stash> Stashes { get; set; } = null!;
        public DbSet<Zone> Zones { get; set; } = null!;
        public DbSet<DungeonRun> DungeonRuns { get; set; } = null!;
        public DbSet<DungeonRoom> DungeonRooms { get; set; } = null!;
        public DbSet<Quest> Quests { get; set; } = null!;
        public DbSet<CharacterQuest> CharacterQuests { get; set; } = null!;

        public PersonsContext(DbContextOptions<PersonsContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Existing entity configuration
            modelBuilder.ApplyConfiguration(new PersonConfiguration());

            // Messaging & notification configurations
            modelBuilder.ApplyConfiguration(new AppUserConfiguration());
            modelBuilder.ApplyConfiguration(new ConversationConfiguration());
            modelBuilder.ApplyConfiguration(new ConversationParticipantConfiguration());
            modelBuilder.ApplyConfiguration(new MessageConfiguration());
            modelBuilder.ApplyConfiguration(new NotificationConfiguration());
            modelBuilder.ApplyConfiguration(new NotificationPreferenceConfiguration());

            // RPG Game Engine configurations
            modelBuilder.ApplyConfiguration(new CharacterConfiguration());
            modelBuilder.ApplyConfiguration(new CharacterStatsConfiguration());
            modelBuilder.ApplyConfiguration(new SkillConfiguration());
            modelBuilder.ApplyConfiguration(new LearnedSkillConfiguration());
            modelBuilder.ApplyConfiguration(new BattleConfiguration());
            modelBuilder.ApplyConfiguration(new BattleParticipantConfiguration());
            modelBuilder.ApplyConfiguration(new BattleTurnLogConfiguration());
            modelBuilder.ApplyConfiguration(new ActiveStatusEffectConfiguration());
            modelBuilder.ApplyConfiguration(new EnemyConfiguration());
            modelBuilder.ApplyConfiguration(new EnemySkillConfiguration());
            modelBuilder.ApplyConfiguration(new LootTableEntryConfiguration());
            modelBuilder.ApplyConfiguration(new ItemConfiguration());
            modelBuilder.ApplyConfiguration(new InventoryItemConfiguration());
            modelBuilder.ApplyConfiguration(new EquippedItemConfiguration());
            modelBuilder.ApplyConfiguration(new StashConfiguration());
            modelBuilder.ApplyConfiguration(new ZoneConfiguration());
            modelBuilder.ApplyConfiguration(new DungeonRunConfiguration());
            modelBuilder.ApplyConfiguration(new DungeonRoomConfiguration());
            modelBuilder.ApplyConfiguration(new QuestConfiguration());
            modelBuilder.ApplyConfiguration(new CharacterQuestConfiguration());

            // Seed RPG game data (zones, skills, enemies, items, loot tables, quests)
            modelBuilder.SeedGameData();
        }
    }
}
