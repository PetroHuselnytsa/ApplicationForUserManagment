using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestFirstProject.Models;

namespace TestFirstProject.Configurations
{
    public class RoleConfiguration : IEntityTypeConfiguration<Role>
    {
        // Pre-defined role IDs for seeding — deterministic so migrations are idempotent
        public static readonly Guid AdminRoleId = new("a1b2c3d4-e5f6-7890-abcd-ef1234567890");
        public static readonly Guid UserRoleId = new("b2c3d4e5-f6a7-8901-bcde-f12345678901");
        public static readonly Guid PremiumUserRoleId = new("c3d4e5f6-a7b8-9012-cdef-123456789012");

        public void Configure(EntityTypeBuilder<Role> builder)
        {
            builder.ToTable("auth_roles");

            builder.HasKey(r => r.Id);

            builder.Property(r => r.Id)
                   .HasColumnName("id")
                   .HasDefaultValueSql("gen_random_uuid()");

            builder.Property(r => r.Name)
                   .HasColumnName("name")
                   .IsRequired()
                   .HasMaxLength(50);

            // Unique index on role name
            builder.HasIndex(r => r.Name)
                   .IsUnique()
                   .HasDatabaseName("IX_auth_roles_name");

            // Seed default roles
            builder.HasData(
                new Role("Admin") { Id = AdminRoleId },
                new Role("User") { Id = UserRoleId },
                new Role("PremiumUser") { Id = PremiumUserRoleId }
            );
        }
    }
}
