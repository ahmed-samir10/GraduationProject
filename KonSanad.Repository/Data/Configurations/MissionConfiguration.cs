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

            // Mark Title, Address and PhoneNumber optional to match DB nulls
            builder.Property(m => m.Title).IsRequired(false).HasMaxLength(200);
            builder.Property(m => m.Description).IsRequired(false).HasMaxLength(1000);

            builder.Property(m => m.Address).IsRequired(false).HasMaxLength(500);
            builder.Property(m => m.PhoneNumber).IsRequired(false).HasMaxLength(50);

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
