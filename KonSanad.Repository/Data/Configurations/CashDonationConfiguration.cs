namespace KonSanad.Repository.Data.Configurations
{
    public class CashDonationConfiguration : IEntityTypeConfiguration<CashDonation>
    {
        public void Configure(EntityTypeBuilder<CashDonation> builder)
        {
            builder.ToTable("CashDonation");
            builder.HasKey(cd => cd.Id);

            builder.Property(cd => cd.Amount).IsRequired().HasColumnType("decimal(18,2)");

            // Relationship with DonationOrder (One-to-One)
            builder.HasOne(cd => cd.DonationOrder)
                .WithOne(d => d.CashDonation)
                .HasForeignKey<CashDonation>(cd => cd.DonationOrderId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
