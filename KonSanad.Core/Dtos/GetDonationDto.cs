using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KonSanad.Core.Entity;

namespace KonSanad.Core.Dtos
{
    // DTO للتبرع النقدي
    public class CashDonationDto
    {
        public int Id { get; set; }
        public DateTime? OrderDate { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string? Status { get; set; }
        public decimal CashAmount { get; set; }
        public int CampaignId { get; set; }
        public int HelpOrderId { get; set; }
        public List<int> ReceiptIds { get; set; } = new List<int>();

        public CashDonationStatus cashDonationStatus { get; set; }
    }

    // DTO لعنصر واحد من التبرع العيني
    public class DonationItemDetailDto
    {
        public int SupplyId { get; set; }
        public int Quantity { get; set; }
        public string? CustomName { get; set; }
        public string? ImageFile { get; set; }
        public string Unit { get; set; } = string.Empty;
    }

    // DTO للتبرع العيني (Order كامل)
    public class ItemDonationDto
    {
        public int Id { get; set; }
        public DateTime? OrderDate { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string? Status { get; set; }
        public List<DonationItemDetailDto> Items { get; set; } = new List<DonationItemDetailDto>();
        public List<int> ReceiptIds { get; set; } = new List<int>();
    }
}
