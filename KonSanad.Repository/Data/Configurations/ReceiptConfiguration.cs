using KonSanad.Repository.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KonSanad.Repository.Data.Configurations
{
    public class ReceiptConfiguration : IEntityTypeConfiguration<Receipt>
    {
        public void Configure(EntityTypeBuilder<Receipt> builder)
        {
            builder.ToTable("Receipts");
            builder.HasKey(r => r.Id);

            // Relationship with DonationOrder
            builder.HasOne(r => r.DonationOrder)
                .WithMany(d => d.Receipts)
                .HasForeignKey(r => r.DonationOrderId)
                .OnDelete(DeleteBehavior.NoAction);

            // Relationship with Manager
            builder.HasOne(r => r.Manager)
                .WithMany(m => m.Receipts)
                .HasForeignKey(r => r.ManagerId)
                .OnDelete(DeleteBehavior.NoAction);

            // Relationship with Beneficiary
            builder.HasOne(r => r.Beneficiary)
                .WithMany(b => b.Receipts)
                .HasForeignKey(r => r.BeneficiaryId)
                .OnDelete(DeleteBehavior.NoAction);

            // Relationship with Donor
            builder.HasOne(r => r.Donor)
                .WithMany(d => d.Receipts)
                .HasForeignKey(r => r.DonorId)
                .OnDelete(DeleteBehavior.NoAction);

            // Relationship with CashDonation
            builder.HasOne(r => r.CashDonation)
                .WithMany()
                .HasForeignKey(r => r.CashDonationId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
