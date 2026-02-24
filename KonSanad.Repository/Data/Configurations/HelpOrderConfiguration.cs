namespace KonSanad.Repository.Data.Configurations;

public class HelpOrderConfiguration : IEntityTypeConfiguration<HelpOrder>
{
    public void Configure(EntityTypeBuilder<HelpOrder> builder)
    {
        builder.ToTable("HelpOrders");
        builder.HasKey(h => h.Id);

        builder.Property(h => h.Description).IsRequired(false).HasMaxLength(1000);

        // Relationship with Beneficiary
        builder.HasOne(h => h.Beneficiary)
            .WithMany(b => b.HelpOrders)
            .HasForeignKey(h => h.BeneficiaryId)
            .OnDelete(DeleteBehavior.NoAction);

        // Relationship with Manager
        builder.HasOne(h => h.Manager)
            .WithMany(m => m.HelpOrders)
            .HasForeignKey(h => h.ManagerId)
            .OnDelete(DeleteBehavior.NoAction);

        // Navigation properties
        builder.HasMany(h => h.HelpOrderItems)
            .WithOne(hoi => hoi.HelpOrder)
            .HasForeignKey(hoi => hoi.HelpOrderId)
            .OnDelete(DeleteBehavior.NoAction);

        // Enum mapping for HelpOrderStatus
        builder.Property(h => h.HelpOrderStatus)
       .IsRequired()
       .HasConversion<int>()               // Explicit enum -> int mapping
       .HasDefaultValue(HelpOrderStatus.Pending);
    }
}