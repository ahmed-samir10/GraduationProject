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
    public class DonationOrderConfiguration : IEntityTypeConfiguration<DonationOrder>
    {
        public void Configure(EntityTypeBuilder<DonationOrder> builder)
        {
            builder.ToTable("DonationOrders");
            builder.HasKey(d => d.Id);

            builder.Property(d => d.DonationCategoryStatus).IsRequired().HasMaxLength(50);
            builder.Property(d => d.CashAmount).HasColumnType("decimal(18,2)");
            builder.Property(d => d.OrderDate).IsRequired(false);

            // Relationship with Donor
            builder.HasOne(d => d.Donor)
                .WithMany(don => don.DonationOrders)
                .HasForeignKey(d => d.DonorId)
                .OnDelete(DeleteBehavior.NoAction);

            // Relationship with Campaign
            builder.HasOne(d => d.Campaign)
                .WithMany(c => c.DonationOrders)
                .HasForeignKey(d => d.CampaignId)
                .OnDelete(DeleteBehavior.NoAction);

            // Relationship with Manager
            builder.HasOne(d => d.Manager)
                .WithMany(m => m.DonationOrders)
                .HasForeignKey(d => d.ManagerId)
                .OnDelete(DeleteBehavior.NoAction);

            // Navigation properties
            builder.HasMany(d => d.OrderItems)
                .WithOne(oi => oi.DonationOrder)
                .HasForeignKey(oi => oi.DonationOrderId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasMany(d => d.Receipts)
                .WithOne(r => r.DonationOrder)
                .HasForeignKey(r => r.DonationOrderId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
