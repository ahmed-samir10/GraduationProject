using KonSanad.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KonSanad.Repository.Data.Configurations
{
    internal class CampaignVolunteerConfiguration : IEntityTypeConfiguration<CampaignVolunteer>
    {
        public void Configure(EntityTypeBuilder<CampaignVolunteer> builder)
        {
            builder.ToTable("CampaignVolunteers");

            // Primary Key
            builder.HasKey(cv => cv.Id);

            // Properties Configuration
            builder.Property(cv => cv.JoinedDate).IsRequired(false);
            builder.Property(cv => cv.Status).HasMaxLength(50);
            builder.Property(cv => cv.Notes).HasMaxLength(500);

            // Relationship with Campaign
            builder.HasOne(cv => cv.Campaign)
                .WithMany(c => c.CampaignVolunteers)
                .HasForeignKey(cv => cv.CampaignId)
                .OnDelete(DeleteBehavior.NoAction);

            // Relationship with Volunteer
            builder.HasOne(cv => cv.Volunteer)
                .WithMany(v => v.CampaignVolunteers)
                .HasForeignKey(cv => cv.VolunteerId)
                .OnDelete(DeleteBehavior.NoAction);

            // Composite Unique Index (Optional but Recommended)
            // يمنع نفس المتطوع من الانضمام لنفس الحملة مرتين
            builder.HasIndex(cv => new { cv.CampaignId, cv.VolunteerId })
                .IsUnique()
                .HasDatabaseName("IX_CampaignVolunteer_Campaign_Volunteer");
        }
    }
}
