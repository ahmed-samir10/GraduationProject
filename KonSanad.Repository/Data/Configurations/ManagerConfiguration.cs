using KonSanad.Repository.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KonSanad.Repository.Data.Configurations
{
    public class ManagerConfiguration : IEntityTypeConfiguration<Manager>
    {
        public void Configure(EntityTypeBuilder<Manager> builder)
        {
            builder.ToTable("Managers");
            builder.HasKey(m => m.Id);

            builder.Property(m => m.FullName).IsRequired().HasMaxLength(200);
            builder.Property(m => m.Email).IsRequired().HasMaxLength(200);
            builder.Property(m => m.Phone).IsRequired().HasMaxLength(50);
            builder.Property(m => m.Password).IsRequired().HasMaxLength(200);
            builder.Property(m => m.Address).IsRequired().HasMaxLength(500);

            // Relationships
            builder.HasMany(m => m.DonationOrders)
                .WithOne(d => d.Manager)
                .HasForeignKey(d => d.ManagerId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasMany(m => m.Missions)
                .WithOne(mis => mis.Manager)
                .HasForeignKey(mis => mis.ManagerId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasMany(m => m.HelpOrders)
                .WithOne(h => h.Manager)
                .HasForeignKey(h => h.ManagerId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasMany(m => m.Receipts)
                .WithOne(r => r.Manager)
                .HasForeignKey(r => r.ManagerId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasMany(m => m.Campaigns)
                .WithOne(c => c.Manager)
                .HasForeignKey(c => c.ManagerId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasMany(m => m.SupplyMovements)
                .WithOne(sm => sm.Manager)
                .HasForeignKey(sm => sm.ManagerId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasMany(m => m.Volunteers)
                .WithOne(v => v.Manager)
                .HasForeignKey(v => v.ManagerId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
