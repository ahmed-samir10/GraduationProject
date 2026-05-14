namespace KonSanad.Repository.Data.Configurations
{
    public class CampaignResultConfiguration : IEntityTypeConfiguration<CampaignResult>
    {
        public void Configure(EntityTypeBuilder<CampaignResult> builder)
        {
            builder.ToTable("CampaignResult");
            builder.HasKey(cr => cr.Id);

           
            builder.Property(cr => cr.TotalCashCollected).HasColumnType("decimal(18,2)");
          

            // Relationship with Campaign
            builder.HasOne(cr => cr.Campaign)
                .WithMany(c => c.CampaignResults)
                .HasForeignKey(cr => cr.CampaignId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
