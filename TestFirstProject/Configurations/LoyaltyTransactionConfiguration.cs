using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestFirstProject.Models;

namespace TestFirstProject.Configurations;

public class LoyaltyTransactionConfiguration : IEntityTypeConfiguration<LoyaltyTransaction>
{
    public void Configure(EntityTypeBuilder<LoyaltyTransaction> builder)
    {
        builder.ToTable("loyalty_transactions");

        builder.HasKey(l => l.Id);
        builder.Property(l => l.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(l => l.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(l => l.Points)
            .HasColumnName("points")
            .IsRequired();

        builder.Property(l => l.Description)
            .HasColumnName("description")
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(l => l.OrderId)
            .HasColumnName("order_id");

        builder.Property(l => l.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.HasOne(l => l.User)
            .WithMany()
            .HasForeignKey(l => l.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(l => l.UserId);
        builder.HasIndex(l => l.OrderId);
    }
}
