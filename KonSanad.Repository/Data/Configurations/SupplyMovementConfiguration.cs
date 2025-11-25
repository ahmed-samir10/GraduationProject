using KonSanad.Repository.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KonSanad.Repository.Data.Configurations
{
    public class SupplyMovementConfiguration : IEntityTypeConfiguration<SupplyMovement>
    {
        public void Configure(EntityTypeBuilder<SupplyMovement> builder)
        {
            builder.ToTable("SuppliesMovement");
            builder.HasKey(sm => sm.Id);

            builder.Property(sm => sm.MovementType).IsRequired().HasMaxLength(50);
            builder.Property(sm => sm.MovedQuantity).IsRequired(false);
            builder.Property(sm => sm.MovementDate).IsRequired(false);

            // Relationship with Supply
            builder.HasOne(sm => sm.Supply)
                .WithMany(s => s.SupplyMovements)
                .HasForeignKey(sm => sm.SupplyId)
                .OnDelete(DeleteBehavior.NoAction);

            // Relationship with OrderItem
            builder.HasOne(sm => sm.OrderItem)
                .WithMany()
                .HasForeignKey(sm => sm.OrderItemId)
                .OnDelete(DeleteBehavior.NoAction);

            // Relationship with Beneficiary
            builder.HasOne(sm => sm.Beneficiary)
                .WithMany(b => b.SupplyMovements)
                .HasForeignKey(sm => sm.BeneficiaryID)
                .OnDelete(DeleteBehavior.NoAction);

            // Relationship with Manager
            builder.HasOne(sm => sm.Manager)
                .WithMany(m => m.SupplyMovements)
                .HasForeignKey(sm => sm.ManagerId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
