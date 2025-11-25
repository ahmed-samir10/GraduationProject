using KonSanad.Repository.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KonSanad.Repository.Data.Configurations
{
    public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
    {
        public void Configure(EntityTypeBuilder<OrderItem> builder)
        {
            builder.ToTable("OrderItems");
            builder.HasKey(oi => oi.Id);

            builder.Property(oi => oi.CustomSupplyName).IsRequired().HasMaxLength(200);
            builder.Property(oi => oi.Quantity).IsRequired();
            builder.Property(oi => oi.Unit).IsRequired().HasMaxLength(50);

            // Relationship with DonationOrder
            builder.HasOne(oi => oi.DonationOrder)
                .WithMany(d => d.OrderItems)
                .HasForeignKey(oi => oi.DonationOrderId)
                .OnDelete(DeleteBehavior.NoAction);

            // Relationship with Supply
            builder.HasOne(oi => oi.Supply)
                .WithMany(s => s.OrderItems)
                .HasForeignKey(oi => oi.SupplyId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
