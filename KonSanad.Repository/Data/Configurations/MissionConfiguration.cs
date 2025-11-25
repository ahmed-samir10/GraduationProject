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

            // Relationship with Volunteer
            builder.HasOne(m => m.Volunteer)
                .WithMany(v => v.Missions)
                .HasForeignKey(m => m.VolunteerId)
                .OnDelete(DeleteBehavior.NoAction);

            // Relationship with Campaign
            builder.HasOne(m => m.Campaign)
                .WithMany(c => c.Missions)
                .HasForeignKey(m => m.CampaignId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
