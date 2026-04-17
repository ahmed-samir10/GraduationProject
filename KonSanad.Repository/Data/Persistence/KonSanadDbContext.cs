 
namespace KonSanad.Repository.Data.Persistence
{
    public class KonSanadDbContext : DbContext
    {
        // My Sring Connection
        // const string ConnectionString = "Server=.;Database=KonSanadDb;Trusted_Connection=True;" +"TrustServerCertificate=True;MultipleActiveResultSets=True;";

        public KonSanadDbContext(DbContextOptions<KonSanadDbContext> options) : base(options)
        { 
        }

        #region My DbSets

        // Actors
        public DbSet<Beneficiary> Beneficiaries { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Donor> Donors { get; set; }

        public DbSet<Manager> Managers { get; set; }
        public DbSet<Volunteer> Volunteers { get; set; }

        // Campaign
        public DbSet<Campaign> Campaigns { get; set; }
        public DbSet<CampaignResult> CampaignResults { get; set; }
        public DbSet<BeneficiaryCampaign> BeneficiaryCampaign { get; set; }
        public DbSet<CampaignVolunteer> CampaignVolunteers { get; set; } // Many To Many

        // Donations
        public DbSet<DonationOrder> DonationOrders { get; set; }
        public DbSet<CashDonation> CashDonations { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

        // Help Orders
        public DbSet<HelpOrder> HelpOrders { get; set; }
        public DbSet<HelpOrderItem> HelpOrderItems { get; set; }
        public DbSet<HelpOrderFile> HelpOrderFiles { get; set; }

        // MyInventory
        public DbSet<Supply> Supplies { get; set; }
        public DbSet<SupplyMovement> SupplyMovements { get; set; }

        // Category
        public DbSet<Category> Categories { get; set; }
        public DbSet<VolunteerMission> VolunteerMissions { get; set; }
        public DbSet<MissionVolunteers> MissionVolunteers { get; set; }


        // Receipt
        public DbSet<Receipt> Receipts { get; set; }

        // Missions
        public DbSet<Mission> Missions { get; set; }

        public DbSet<MissionVolunteers> MissionVolunteer { get; set; }


        

        #endregion

        #region Catch All Configurations In My Assembly => My Project
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            // This Is Way Is More Redabal To Add All Configurations In Your Assembly
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(KonSanadDbContext).Assembly);
        }
        #endregion

        #region  The Many Override To Same Work Action => SaveChanges()
        public override int SaveChanges()
        {
            ApplyAuditColumns();
            return base.SaveChanges();
        }
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            ApplyAuditColumns();
            return base.SaveChangesAsync(cancellationToken);
        }

        #endregion

        #region Automatic Apply Audit Columns
        private void ApplyAuditColumns()
        {
            var cairoZone = TimeZoneInfo.FindSystemTimeZoneById("Egypt Standard Time");
            var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, cairoZone); // ✅ Cairo Time

            foreach (var entry in ChangeTracker.Entries().Where(e => e.State == EntityState.Added))
            {
                if (entry.Properties.FirstOrDefault(p => p.Metadata.Name == "CreatedAt") is { } created)
                {
                    created.CurrentValue = now;
                }
            }

            foreach (var entry in ChangeTracker.Entries().Where(e => e.State == EntityState.Modified))
            {
                if (entry.Properties.FirstOrDefault(p => p.Metadata.Name == "UpdatedAt") is { } updated)
                {
                    updated.CurrentValue = now;
                }
            }
        }
        #endregion

    }
}
