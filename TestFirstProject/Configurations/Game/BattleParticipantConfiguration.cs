using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestFirstProject.Models.Game;

namespace TestFirstProject.Configurations.Game
{
    public class BattleParticipantConfiguration : IEntityTypeConfiguration<BattleParticipant>
    {
        public void Configure(EntityTypeBuilder<BattleParticipant> builder)
        {
            builder.ToTable("game_battle_participants");

            builder.HasKey(bp => bp.Id);

            builder.Property(bp => bp.Id).HasColumnName("id").ValueGeneratedOnAdd();
            builder.Property(bp => bp.BattleId).HasColumnName("battle_id");
            builder.Property(bp => bp.Type).HasColumnName("type").HasConversion<string>().HasMaxLength(20);
            builder.Property(bp => bp.CharacterId).HasColumnName("character_id");
            builder.Property(bp => bp.EnemyId).HasColumnName("enemy_id");
            builder.Property(bp => bp.Name).HasColumnName("name").IsRequired().HasMaxLength(100);
            builder.Property(bp => bp.CurrentHp).HasColumnName("current_hp");
            builder.Property(bp => bp.MaxHp).HasColumnName("max_hp");
            builder.Property(bp => bp.CurrentMp).HasColumnName("current_mp");
            builder.Property(bp => bp.MaxMp).HasColumnName("max_mp");
            builder.Property(bp => bp.Attack).HasColumnName("attack");
            builder.Property(bp => bp.Defense).HasColumnName("defense");
            builder.Property(bp => bp.MagicPower).HasColumnName("magic_power");
            builder.Property(bp => bp.Speed).HasColumnName("speed");
            builder.Property(bp => bp.CritChance).HasColumnName("crit_chance");
            builder.Property(bp => bp.DodgeChance).HasColumnName("dodge_chance");
            builder.Property(bp => bp.TurnOrder).HasColumnName("turn_order");
            builder.Property(bp => bp.IsPhaseTwo).HasColumnName("is_phase_two").HasDefaultValue(false);

            builder.Ignore(bp => bp.IsAlive);

            builder.HasOne(bp => bp.Battle)
                   .WithMany(b => b.Participants)
                   .HasForeignKey(bp => bp.BattleId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(bp => bp.Character)
                   .WithMany()
                   .HasForeignKey(bp => bp.CharacterId)
                   .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(bp => bp.Enemy)
                   .WithMany()
                   .HasForeignKey(bp => bp.EnemyId)
                   .OnDelete(DeleteBehavior.SetNull);

            builder.HasIndex(bp => bp.BattleId);
        }
    }
}
