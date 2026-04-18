using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestFirstProject.Models;

namespace TestFirstProject.Configurations
{
    /// <summary>
    /// EF Core configuration for the Role entity.
    /// Seeds default roles: Admin, User, PremiumUser.
    /// </summary>
    public class RoleConfiguration : IEntityTypeConfiguration<Role>
    {
        // Well-known role IDs for seeding
        public static readonly Guid AdminRoleId = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890");
        public static readonly Guid UserRoleId = Guid.Parse("b2c3d4e5-f6a7-8901-bcde-f12345678901");
        public static readonly Guid PremiumUserRoleId = Guid.Parse("c3d4e5f6-a7b8-9012-cdef-123456789012");

        public void Configure(EntityTypeBuilder<Role> builder)
        {
            builder.ToTable("auth_roles");

            builder.HasKey(r => r.Id);

            builder.Property(r => r.Id)
                   .HasColumnName("id")
                   .ValueGeneratedOnAdd();

            builder.Property(r => r.Name)
                   .HasColumnName("name")
                   .IsRequired()
                   .HasMaxLength(50);

            // Unique index on role name
            builder.HasIndex(r => r.Name)
                   .IsUnique()
                   .HasDatabaseName("ix_auth_roles_name");

            // Seed default roles
            builder.HasData(
                new Role { Id = AdminRoleId, Name = "Admin" },
                new Role { Id = UserRoleId, Name = "User" },
                new Role { Id = PremiumUserRoleId, Name = "PremiumUser" }
            );
        }
    }
}
