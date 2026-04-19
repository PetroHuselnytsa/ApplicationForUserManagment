using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestFirstProject.Models;

namespace TestFirstProject.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("app_users");

            builder.HasKey(u => u.Id);

            builder.Property(u => u.Id)
                   .HasColumnName("id")
                   .ValueGeneratedOnAdd();

            builder.Property(u => u.Username)
                   .HasColumnName("username")
                   .IsRequired()
                   .HasMaxLength(50);

            builder.Property(u => u.Email)
                   .HasColumnName("email")
                   .IsRequired()
                   .HasMaxLength(255);

            builder.Property(u => u.PasswordHash)
                   .HasColumnName("password_hash")
                   .IsRequired();

            builder.Property(u => u.Role)
                   .HasColumnName("role")
                   .HasConversion<string>()
                   .HasMaxLength(20);

            builder.Property(u => u.CreatedAt)
                   .HasColumnName("created_at");

            // Unique indexes
            builder.HasIndex(u => u.Username).IsUnique();
            builder.HasIndex(u => u.Email).IsUnique();
        }
    }
}
