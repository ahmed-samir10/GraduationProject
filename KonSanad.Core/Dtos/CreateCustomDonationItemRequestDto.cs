using KonSanad.Core.IRepositery;
using KonSanad.Repository.Entities;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace KonSanad.Core.Dtos
{
    public class CreateCustomDonationItemRequestDto
    {
        [Required(ErrorMessage = "اسم العنصر مطلوب")]
        [StringLength(200, ErrorMessage = "اسم العنصر يجب ألا يتجاوز 200 حرف")]
        public string CustomItemName { get; set; } = string.Empty;

        [Required(ErrorMessage = "الكمية مطلوبة")]
        [Range(1, int.MaxValue, ErrorMessage = "الكمية يجب أن تكون أكبر من صفر")]
        public int Quantity { get; set; }

        [StringLength(50, ErrorMessage = "الوحدة يجب ألا تتجاوز 50 حرف")]
        public string? Unit { get; set; } // مثلاً: "قطعة", "كيلو", "لتر"

        public IFormFile? ImageFile { get; set; }

        [StringLength(500, ErrorMessage = "الوصف يجب ألا يتجاوز 500 حرف")]
        public string? Description { get; set; }

        public string? Status { get; set; }
    }


}
