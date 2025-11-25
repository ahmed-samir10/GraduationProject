using KonSanad.Repository.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KonSanad.Repository.Data.Configurations
{
    public class HelpOrderItemConfiguration : IEntityTypeConfiguration<HelpOrderItem>
    {
        public void Configure(EntityTypeBuilder<HelpOrderItem> builder)
        {
            builder.ToTable("HelpOrderItem");
            builder.HasKey(hoi => hoi.Id);

            builder.Property(hoi => hoi.QuantityRequested).IsRequired();

            // Relationship with HelpOrder
            builder.HasOne(hoi => hoi.HelpOrder)
                .WithMany(h => h.HelpOrderItems)
                .HasForeignKey(hoi => hoi.HelpOrderId)
                .OnDelete(DeleteBehavior.NoAction);

            // Relationship with Supply
            builder.HasOne(hoi => hoi.Supply)
                .WithMany()
                .HasForeignKey(hoi => hoi.SupplyId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
