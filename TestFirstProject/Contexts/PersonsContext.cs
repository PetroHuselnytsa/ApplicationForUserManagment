using Microsoft.EntityFrameworkCore;
using TestFirstProject.Configurations;
using TestFirstProject.Models;

namespace TestFirstProject.Contexts
{
    public class PersonsContext : DbContext
    {
        public DbSet<Person> Persons { get; set; } = null!;
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;

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
            modelBuilder.ApplyConfiguration(new PersonConfiguration());
            modelBuilder.ApplyConfiguration(new UserConfiguration());
            modelBuilder.ApplyConfiguration(new RefreshTokenConfiguration());
        }
    }
}
