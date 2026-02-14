using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KonSanad.Core.Dtos
{
    public class HelpOrderDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; } = string.Empty;
        public int? BeneficiaryId { get; set; }
        public string Reason { get; set; } = string.Empty;

        public string PictureUrl { get; set; }

        // public List<string> ImageUrls { get; set; } = new List<string>();
        //  public List<HelpOrderItemDto> Items { get; set; } = new List<HelpOrderItemDto>();

        public DateTime? CreatedAt { get; set; }
        public string? Status { get; set; }
      //  public int? ManagerId { get; set; }
    }
}
