using Microsoft.EntityFrameworkCore;
using TestFirstProject.Configurations;
using TestFirstProject.Models;

namespace TestFirstProject.Contexts
{
    public class PersonsContext : DbContext
    {
        public DbSet<Person> Persons { get; set; } = null!;
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Role> Roles { get; set; } = null!;
        public DbSet<UserRole> UserRoles { get; set; } = null!;
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
            // Existing entity configuration
            modelBuilder.ApplyConfiguration(new PersonConfiguration());

            // Auth entity configurations
            modelBuilder.ApplyConfiguration(new UserConfiguration());
            modelBuilder.ApplyConfiguration(new RoleConfiguration());
            modelBuilder.ApplyConfiguration(new UserRoleConfiguration());
            modelBuilder.ApplyConfiguration(new RefreshTokenConfiguration());
        }
    }
}
