using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestFirstProject.Models;

namespace TestFirstProject.Configurations
{
    public class PersonConfiguration : IEntityTypeConfiguration<Person>
    {
        public void Configure(EntityTypeBuilder<Person> builder)
        {
            builder.ToTable("users");

            builder.Property(p => p.Name)
                   .HasColumnName("name")
                   .IsRequired()
                   .HasMaxLength(14);

            builder.HasKey(p => p.Id);

            builder.Property(p => p.Id)
                   .HasColumnName("id");

            builder.Property(p => p.Age)
                   .IsRequired()
                   .HasColumnName("age");
        }
    }
}
