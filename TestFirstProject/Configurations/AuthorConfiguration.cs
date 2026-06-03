using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestFirstProject.Models;

namespace TestFirstProject.Configurations
{
    public class AuthorConfiguration : IEntityTypeConfiguration<Author>
    {
        public void Configure(EntityTypeBuilder<Author> builder)
        {
            builder.ToTable("authors");

            builder.HasKey(a => a.Id);

            builder.Property(a => a.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();

            builder.Property(a => a.Name)
                .HasColumnName("name")
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(a => a.Biography)
                .HasColumnName("biography")
                .HasMaxLength(4000);

            builder.Property(a => a.Country)
                .HasColumnName("country")
                .HasMaxLength(100);

            builder.Property(a => a.CreatedAt)
                .HasColumnName("created_at");

            // Indexes
            builder.HasIndex(a => a.Name);
        }
    }
}
