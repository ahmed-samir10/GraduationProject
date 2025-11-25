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
    public class BeneficiaryConfiguration : IEntityTypeConfiguration<Beneficiary>
    {
        public void Configure(EntityTypeBuilder<Beneficiary> builder)
        {
            builder.ToTable("Beneficiaries");
            builder.HasKey(b => b.Id);

            builder.Property(b => b.FullName).IsRequired().HasMaxLength(200);
            builder.Property(b => b.Email).IsRequired().HasMaxLength(200);
            builder.Property(b => b.Pssword).IsRequired().HasMaxLength(200);
            builder.Property(b => b.Phone).IsRequired().HasMaxLength(50);
            builder.Property(b => b.Address).IsRequired().HasMaxLength(500);

            // Relationships
            builder.HasMany(b => b.HelpOrders)
                .WithOne(h => h.Beneficiary)
                .HasForeignKey(h => h.BeneficiaryId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasMany(b => b.Receipts)
                .WithOne(r => r.Beneficiary)
                .HasForeignKey(r => r.BeneficiaryId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasMany(b => b.SupplyMovements)
                .WithOne(sm => sm.Beneficiary)
                .HasForeignKey(sm => sm.BeneficiaryID)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
