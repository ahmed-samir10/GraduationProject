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
    public class CampaignResultConfiguration : IEntityTypeConfiguration<CampaignResult>
    {
        public void Configure(EntityTypeBuilder<CampaignResult> builder)
        {
            builder.ToTable("CampaignResult");
            builder.HasKey(cr => cr.Id);

            builder.Property(cr => cr.ReportDate).IsRequired(false);
            builder.Property(cr => cr.TotalCashCollected).HasColumnType("decimal(18,2)");
            builder.Property(cr => cr.TotalItemsCount).IsRequired(false);

            // Relationship with Campaign
            builder.HasOne(cr => cr.Campaign)
                .WithMany(c => c.CampaignResults)
                .HasForeignKey(cr => cr.CampaignId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
