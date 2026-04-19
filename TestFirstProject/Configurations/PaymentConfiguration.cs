using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestFirstProject.Models;

namespace TestFirstProject.Configurations
{
    public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
    {
        public void Configure(EntityTypeBuilder<Payment> builder)
        {
            builder.ToTable("payments");

            builder.HasKey(p => p.Id);
            builder.Property(p => p.Id).HasColumnName("id").ValueGeneratedOnAdd();
            builder.Property(p => p.OrderId).HasColumnName("order_id");
            builder.Property(p => p.Method).HasColumnName("method").HasConversion<string>().HasMaxLength(20);
            builder.Property(p => p.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(20);
            builder.Property(p => p.Amount).HasColumnName("amount").HasColumnType("decimal(18,2)");
            builder.Property(p => p.RefundedAmount).HasColumnName("refunded_amount").HasColumnType("decimal(18,2)");
            builder.Property(p => p.TransactionId).HasColumnName("transaction_id").HasMaxLength(100);
            builder.Property(p => p.CreatedAt).HasColumnName("created_at");
            builder.Property(p => p.UpdatedAt).HasColumnName("updated_at");

            builder.HasOne(p => p.Order).WithMany(o => o.Payments).HasForeignKey(p => p.OrderId).OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(p => p.OrderId);
            builder.HasIndex(p => p.TransactionId);
        }
    }
}
