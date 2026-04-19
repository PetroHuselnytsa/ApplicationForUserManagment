using Microsoft.EntityFrameworkCore;
using TestFirstProject.Configurations;
using TestFirstProject.Models;

namespace TestFirstProject.Contexts
{
    public class PersonsContext : DbContext
    {
        // Existing entity
        public DbSet<Person> Persons { get; set; } = null!;

        // New entities for messaging & notification system
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Conversation> Conversations { get; set; } = null!;
        public DbSet<ConversationParticipant> ConversationParticipants { get; set; } = null!;
        public DbSet<Message> Messages { get; set; } = null!;
        public DbSet<Notification> Notifications { get; set; } = null!;
        public DbSet<NotificationPreference> NotificationPreferences { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var configuration = new ConfigurationBuilder()
                                .AddJsonFile("appsettings.json")
                                .SetBasePath(Directory.GetCurrentDirectory())
                                .Build();

            string connectionString = configuration.GetConnectionString("PostgresConnection") ?? null!;
            optionsBuilder.UseNpgsql(connectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Existing configuration
            modelBuilder.ApplyConfiguration(new PersonConfiguration());

            // New configurations
            modelBuilder.ApplyConfiguration(new UserConfiguration());
            modelBuilder.ApplyConfiguration(new ConversationConfiguration());
            modelBuilder.ApplyConfiguration(new ConversationParticipantConfiguration());
            modelBuilder.ApplyConfiguration(new MessageConfiguration());
            modelBuilder.ApplyConfiguration(new NotificationConfiguration());
            modelBuilder.ApplyConfiguration(new NotificationPreferenceConfiguration());
        }
    }
}
