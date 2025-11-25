using KonSanad.Repository.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KonSanad.Repository.Data.Configurations
{
    public class CampaignConfiguration : IEntityTypeConfiguration<Campaign>
    {
        public void Configure(EntityTypeBuilder<Campaign> builder)
        {
            builder.ToTable("Campaigns");
            builder.HasKey(c => c.Id);

            builder.Property(c => c.Title).IsRequired().HasMaxLength(200);
            builder.Property(c => c.Description).IsRequired().HasMaxLength(1000);
            builder.Property(c => c.StartDate).IsRequired(false);
            builder.Property(c => c.EndDate).IsRequired(false);
            builder.Property(c => c.GoalCash).HasColumnType("decimal(18,2)");
            builder.Property(c => c.GoalSupplies).IsRequired(false);

            // Relationships with Category
            builder.HasOne(c => c.Category)
                .WithMany(cat => cat.Campaigns)
                .HasForeignKey(c => c.CategoryId)
                .OnDelete(DeleteBehavior.NoAction);

            // Relationships with Manager
            builder.HasOne(c => c.Manager)
                .WithMany(m => m.Campaigns)
                .HasForeignKey(c => c.ManagerId)
                .OnDelete(DeleteBehavior.NoAction);

            // Navigation properties
            builder.HasMany(c => c.DonationOrders)
                .WithOne(d => d.Campaign)
                .HasForeignKey(d => d.CampaignId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasMany(c => c.CampaignResults)
                .WithOne(cr => cr.Campaign)
                .HasForeignKey(cr => cr.CampaignId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasMany(c => c.Missions)
                .WithOne(m => m.Campaign)
                .HasForeignKey(m => m.CampaignId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasMany(c => c.CampaignVolunteers)
                 .WithOne(cv => cv.Campaign)
                 .HasForeignKey(cv => cv.CampaignId)
                 .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
