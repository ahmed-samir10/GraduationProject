// KonSanad.Repository\Entities\Manager.cs
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

namespace KonSanad.Repository.Entities
{
    [Table("Managers")]
    public class Manager : BaseEntity<int>
    {
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string Address { get; set; } = null!;



        // Navigation
        public ICollection<DonationOrder> DonationOrders { get; set; } = new List<DonationOrder>();
        public ICollection<Mission> Missions { get; set; } = new List<Mission>();
        public ICollection<HelpOrder> HelpOrders { get; set; } = new List<HelpOrder>();
        public ICollection<Receipt> Receipts { get; set; } = new List<Receipt>();
        public ICollection<Campaign> Campaigns { get; set; } = new List<Campaign>();

        public ICollection<SupplyMovement> SupplyMovements { get; set; } = new List<SupplyMovement>();

        public ICollection<Volunteer> Volunteers { get; set; } = new List<Volunteer>();
    }
}