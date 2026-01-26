using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace KonSanad.Core.Dtos
{
    //public class DonationItemDto
    //{
    //    public int SupplyId { get; set; }
    //    public int Quantity { get; set; }
    //   // public string? CustomName { get; set; }
    //    //public string? ImageFile { get; set; }
    //    //public string Unit { get; set; } = string.Empty;
    //}
    public class DonationItemDto
    {

        public int SupplyId { get; set; }
        //public string? ImageFile { get; set; }
        public int Quantity { get; set; }


        // include these in the response DTO so UI can show them
        public string? Status { get; set; }
        public string? Description { get; set; }
    }
}