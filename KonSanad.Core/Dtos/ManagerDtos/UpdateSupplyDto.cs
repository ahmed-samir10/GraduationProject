using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace KonSanad.Core.Dtos.ManagerDtos
{
    // UpdateSupplyDto.cs
    public class UpdateSupplyDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Unit { get; set; }
        public int? StockQuantity { get; set; }
        public int? MinimumLevel { get; set; }
        public string? Title { get; set; }
        public IFormFile? Image { get; set; }   // ✅ الصورة بتيجي من الـ Form
        public string? ImageUrl { get; set; }   // ✅ بيتملى في الـ Controller بعد الحفظ
    }
}
