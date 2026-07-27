using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestFirstProject.Models;

namespace TestFirstProject.Configurations
{
    public class PlayerRatingConfiguration : IEntityTypeConfiguration<PlayerRating>
    {
        public void Configure(EntityTypeBuilder<PlayerRating> builder)
        {
            builder.ToTable("player_ratings");

            builder.HasKey(r => r.Id);

            builder.Property(r => r.Id)
                   .HasColumnName("id")
                   .ValueGeneratedOnAdd();

            builder.Property(r => r.UserId)
                   .HasColumnName("user_id");

            builder.Property(r => r.Rating)
                   .HasColumnName("rating")
                   .HasDefaultValue(1000);

            builder.Property(r => r.GamesPlayed)
                   .HasColumnName("games_played")
                   .HasDefaultValue(0);

            builder.Property(r => r.HighestScore)
                   .HasColumnName("highest_score")
                   .HasDefaultValue(0);

            builder.Property(r => r.LastPlayedAt)
                   .HasColumnName("last_played_at");

            // FK to auth_users — one rating per user
            builder.HasOne(r => r.User)
                   .WithMany()
                   .HasForeignKey(r => r.UserId)
                   .OnDelete(DeleteBehavior.Cascade);

            // Enforce one rating per user
            builder.HasIndex(r => r.UserId).IsUnique();

            // Index for rating leaderboard
            builder.HasIndex(r => r.Rating);
        }
    }
}
