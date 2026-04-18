using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestFirstProject.Models;

namespace TestFirstProject.Configurations
{
    public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
    {
        public void Configure(EntityTypeBuilder<UserRole> builder)
        {
            builder.ToTable("auth_user_roles");

            // Composite primary key
            builder.HasKey(ur => new { ur.UserId, ur.RoleId });

            builder.Property(ur => ur.UserId)
                   .HasColumnName("user_id");

            builder.Property(ur => ur.RoleId)
                   .HasColumnName("role_id");

            // Relationships
            builder.HasOne(ur => ur.User)
                   .WithMany(u => u.UserRoles)
                   .HasForeignKey(ur => ur.UserId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(ur => ur.Role)
                   .WithMany(r => r.UserRoles)
                   .HasForeignKey(ur => ur.RoleId)
                   .OnDelete(DeleteBehavior.Cascade);

            // Index for faster role lookups by user
            builder.HasIndex(ur => ur.UserId)
                   .HasDatabaseName("IX_auth_user_roles_user_id");

            builder.HasIndex(ur => ur.RoleId)
                   .HasDatabaseName("IX_auth_user_roles_role_id");
        }
    }
}
