using KonSanad.Repository.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KonSanad.Repository.Data.Configurations
{
    public class MissionConfiguration : IEntityTypeConfiguration<Mission>
    {
        public void Configure(EntityTypeBuilder<Mission> builder)
        {
            builder.ToTable("Missions");
            builder.HasKey(m => m.Id);

            builder.Property(m => m.Title).IsRequired().HasMaxLength(200);
            builder.Property(m => m.Description).IsRequired().HasMaxLength(1000);
            builder.Property(m => m.StartDate).IsRequired(false);
            builder.Property(m => m.EndDate).IsRequired(false);

            // Relationship with Manager
            builder.HasOne(m => m.Manager)
                .WithMany(man => man.Missions)
                .HasForeignKey(m => m.ManagerId)
                .OnDelete(DeleteBehavior.NoAction);

            // ----------------------------
            // Mission ↔ VolunteerMission
            // ----------------------------
            builder.HasMany(m => m.VolunteerMissions)
                   .WithOne(vm => vm.Mission)
                   .HasForeignKey(vm => vm.MissionId)
                   .OnDelete(DeleteBehavior.NoAction);

            // Relationship with Campaign
            builder.HasOne(m => m.Campaign)
                .WithMany(c => c.Missions)
                .HasForeignKey(m => m.CampaignId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
