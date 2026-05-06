namespace KonSanad.Repository.Data.Configurations
{
    public class CategoryConfiguration : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> builder)
        {
            builder.ToTable("Categories");
            builder.HasKey(c => c.Id);

            builder.Property(c => c.Name).IsRequired().HasMaxLength(100);
            builder.Property(c => c.Description).HasMaxLength(500);

            // Relationships
           /* builder.HasMany(c => c.Supplies)
                .WithOne(s => s.Category)
                .HasForeignKey(s => s.CategoryId)
                .OnDelete(DeleteBehavior.NoAction);*/

            builder.HasMany(c => c.Campaigns)
                .WithOne(camp => camp.Category)
                .HasForeignKey(camp => camp.CategoryId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
