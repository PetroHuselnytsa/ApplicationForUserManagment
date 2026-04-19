using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestFirstProject.Models.Game;

namespace TestFirstProject.Configurations.Game
{
    public class CharacterStatsConfiguration : IEntityTypeConfiguration<CharacterStats>
    {
        public void Configure(EntityTypeBuilder<CharacterStats> builder)
        {
            builder.ToTable("game_character_stats");

            builder.HasKey(s => s.Id);

            builder.Property(s => s.Id)
                   .HasColumnName("id")
                   .ValueGeneratedOnAdd();

            builder.Property(s => s.CharacterId)
                   .HasColumnName("character_id");

            builder.Property(s => s.HP)
                   .HasColumnName("hp");

            builder.Property(s => s.MaxHP)
                   .HasColumnName("max_hp");

            builder.Property(s => s.MP)
                   .HasColumnName("mp");

            builder.Property(s => s.MaxMP)
                   .HasColumnName("max_mp");

            builder.Property(s => s.Attack)
                   .HasColumnName("attack");

            builder.Property(s => s.Defense)
                   .HasColumnName("defense");

            builder.Property(s => s.MagicPower)
                   .HasColumnName("magic_power");

            builder.Property(s => s.Speed)
                   .HasColumnName("speed");

            builder.Property(s => s.CritChance)
                   .HasColumnName("crit_chance");

            builder.Property(s => s.DodgeChance)
                   .HasColumnName("dodge_chance");

            // Relationships — one-to-one with Character
            builder.HasOne(s => s.Character)
                   .WithOne(c => c.Stats)
                   .HasForeignKey<CharacterStats>(s => s.CharacterId)
                   .OnDelete(DeleteBehavior.Cascade);

            // Unique index on CharacterId to enforce one-to-one
            builder.HasIndex(s => s.CharacterId).IsUnique();
        }
    }
}
