using Microsoft.EntityFrameworkCore;
using TestFirstProject.Configurations;
using TestFirstProject.Models;

namespace TestFirstProject.Contexts
{
    public class PersonsContext : DbContext
    {
        // Existing entity
        public DbSet<Person> Persons { get; set; } = null!;

        // Auth entities
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;
        public DbSet<EmailVerificationToken> EmailVerificationTokens { get; set; } = null!;
        public DbSet<PasswordResetToken> PasswordResetTokens { get; set; } = null!;

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

            // Auth configurations
            modelBuilder.ApplyConfiguration(new UserConfiguration());
            modelBuilder.ApplyConfiguration(new RefreshTokenConfiguration());
            modelBuilder.ApplyConfiguration(new EmailVerificationTokenConfiguration());
            modelBuilder.ApplyConfiguration(new PasswordResetTokenConfiguration());
        }
    }
}
