// KonSanad.Repository\Entities\Donor.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KonSanad.Repository.Entities
{
    [Table("Donor")]
    public class Donor : BaseEntity<int>
    {
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public string Address { get; set; } = null!;
        public string DonorType { get; set; } = null!;


        // Navigation
        public ICollection<DonationOrder> DonationOrders { get; set; } = new List<DonationOrder>();
        public ICollection<Receipt> Receipts { get; set; } = new List<Receipt>();
    }
}