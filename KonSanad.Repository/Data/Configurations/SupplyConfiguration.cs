using KonSanad.Repository.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KonSanad.Repository.Data.Configurations
{
    public class SupplyConfiguration : IEntityTypeConfiguration<Supply>
    {
        public void Configure(EntityTypeBuilder<Supply> builder)
        {
            builder.ToTable("Supplies");
            builder.HasKey(s => s.Id);

            builder.Property(s => s.Name).IsRequired().HasMaxLength(200);
            builder.Property(s => s.Description).IsRequired().HasMaxLength(1000);
            builder.Property(s => s.Unit).IsRequired().HasMaxLength(50);
            builder.Property(s => s.StockQuantity).IsRequired(false);
            builder.Property(s => s.MinimumLevel).IsRequired(false);

            // Relationship with Category
            builder.HasOne(s => s.Category)
                .WithMany(c => c.Supplies)
                .HasForeignKey(s => s.CategoryId)
                .OnDelete(DeleteBehavior.NoAction);

            // Navigation properties
            builder.HasMany(s => s.OrderItems)
                .WithOne(oi => oi.Supply)
                .HasForeignKey(oi => oi.SupplyId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasMany(s => s.SupplyMovements)
                .WithOne(sm => sm.Supply)
                .HasForeignKey(sm => sm.SupplyId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
