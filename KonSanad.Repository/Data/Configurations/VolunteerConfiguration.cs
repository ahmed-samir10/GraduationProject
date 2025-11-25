using KonSanad.Repository.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KonSanad.Repository.Data.Configurations
{
    public class VolunteerConfiguration : IEntityTypeConfiguration<Volunteer>
    {
        public void Configure(EntityTypeBuilder<Volunteer> builder)
        {
            builder.ToTable("Volunteers");
            builder.HasKey(v => v.Id);

            builder.Property(v => v.FullName).IsRequired().HasMaxLength(200);
            builder.Property(v => v.Email).IsRequired().HasMaxLength(200);
            builder.Property(v => v.Password).IsRequired().HasMaxLength(200);
            builder.Property(v => v.Phone).IsRequired().HasMaxLength(50);
            builder.Property(v => v.Address).IsRequired().HasMaxLength(500);
            builder.Property(v => v.Skills).IsRequired().HasMaxLength(500);
            builder.Property(v => v.Availability).IsRequired(false);

            // Relationship with Manager
            builder.HasOne(v => v.Manager)
                .WithMany(m => m.Volunteers)
                .HasForeignKey(v => v.ManagerId)
                .OnDelete(DeleteBehavior.NoAction);

            // Navigation properties
            builder.HasMany(v => v.CampaignVolunteers)
                .WithOne(cv => cv.Volunteer)
                .HasForeignKey(cv => cv.VolunteerId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasMany(v => v.Missions)
                .WithOne(m => m.Volunteer)
                .HasForeignKey(m => m.VolunteerId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
