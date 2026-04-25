// KonSanad.Repository\Entities\Supply.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KonSanad.Core.Entities;
using KonSanad.Core.Entity.Enums;

namespace KonSanad.Repository.Entities
{
    [Table("Supplies")]
    public class Supply : BaseEntity<int>
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Unit { get; set; }
        public int? StockQuantity { get; set; } = 0;
        public int? MinimumLevel { get; set; } = 5;
        public string? Title { get; set; }
        public ShowinType ShowinType { get; set; }

        public string? ImageUrl { get; set; }
        // Link to Category
        public string? Category { get; set; }
      

        // Navigation
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public ICollection<SupplyMovement> SupplyMovements { get; set; } = new List<SupplyMovement>();
    }
}