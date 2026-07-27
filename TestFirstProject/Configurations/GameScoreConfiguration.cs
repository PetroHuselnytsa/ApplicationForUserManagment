using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestFirstProject.Models;

namespace TestFirstProject.Configurations
{
    public class GameScoreConfiguration : IEntityTypeConfiguration<GameScore>
    {
        public void Configure(EntityTypeBuilder<GameScore> builder)
        {
            builder.ToTable("game_scores");

            builder.HasKey(g => g.Id);

            builder.Property(g => g.Id)
                   .HasColumnName("id")
                   .ValueGeneratedOnAdd();

            builder.Property(g => g.UserId)
                   .HasColumnName("user_id");

            builder.Property(g => g.Score)
                   .HasColumnName("score");

            builder.Property(g => g.Difficulty)
                   .HasColumnName("difficulty")
                   .HasConversion<string>()
                   .HasMaxLength(10);

            builder.Property(g => g.DurationSeconds)
                   .HasColumnName("duration_seconds");

            builder.Property(g => g.EnemiesDefeated)
                   .HasColumnName("enemies_defeated");

            builder.Property(g => g.FoodEaten)
                   .HasColumnName("food_eaten");

            builder.Property(g => g.MaxCombo)
                   .HasColumnName("max_combo");

            builder.Property(g => g.RatingChange)
                   .HasColumnName("rating_change");

            builder.Property(g => g.PlayedAt)
                   .HasColumnName("played_at");

            // FK to auth_users (AppUser maps to "auth_users")
            builder.HasOne(g => g.User)
                   .WithMany()
                   .HasForeignKey(g => g.UserId)
                   .OnDelete(DeleteBehavior.Cascade);

            // Indexes for leaderboard queries
            builder.HasIndex(g => g.Score);
            builder.HasIndex(g => g.PlayedAt);
            builder.HasIndex(g => new { g.UserId, g.PlayedAt });
        }
    }
}
