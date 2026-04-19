using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestFirstProject.Models;

namespace TestFirstProject.Configurations;

public class WalletConfiguration : IEntityTypeConfiguration<Wallet>
{
    public void Configure(EntityTypeBuilder<Wallet> builder)
    {
        builder.ToTable("wallets");

        builder.HasKey(w => w.Id);
        builder.Property(w => w.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(w => w.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(w => w.Balance)
            .HasColumnName("balance")
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(w => w.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(w => w.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.HasOne(w => w.User)
            .WithMany()
            .HasForeignKey(w => w.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // One wallet per user
        builder.HasIndex(w => w.UserId).IsUnique();
    }
}
