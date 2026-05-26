using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KonSanad.Core.Dtos
{
    public class CampaignReportDto
    {
        public string? Title { get; set; }
        public decimal GoalCash { get; set; }
        public decimal CurrentCash { get; set; }
        public DateTime? Date { get; set; }
    }
}
