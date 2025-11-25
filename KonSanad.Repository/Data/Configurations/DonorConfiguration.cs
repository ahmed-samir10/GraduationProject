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
    public class DonorConfiguration : IEntityTypeConfiguration<Donor>
    {
        public void Configure(EntityTypeBuilder<Donor> builder)
        {
            builder.ToTable("Donor");
            builder.HasKey(d => d.Id);

            builder.Property(d => d.FullName).IsRequired().HasMaxLength(200);
            builder.Property(d => d.Email).IsRequired().HasMaxLength(200);
            builder.Property(d => d.Password).IsRequired().HasMaxLength(200);
            builder.Property(d => d.Phone).IsRequired().HasMaxLength(50);
            builder.Property(d => d.Address).IsRequired().HasMaxLength(500);
            builder.Property(d => d.DonorType).IsRequired().HasMaxLength(50);

            // Relationships
            builder.HasMany(d => d.DonationOrders)
                .WithOne(dono => dono.Donor)
                .HasForeignKey(dono => dono.DonorId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasMany(d => d.Receipts)
                .WithOne(r => r.Donor)
                .HasForeignKey(r => r.DonorId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
