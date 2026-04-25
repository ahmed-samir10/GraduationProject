using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KonSanad.Core.Dtos.ManagerDtos
{
    public class GetSupplyDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Unit { get; set; }
        public int? StockQuantity { get; set; }
        public int? MinimumLevel { get; set; }
        public string? ImageUrl { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsLowStock { get; set; }
    }
}
