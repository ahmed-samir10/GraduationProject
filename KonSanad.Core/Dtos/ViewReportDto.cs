using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KonSanad.Core.Dtos
{
    public class ViewReportDto
    {
        public int missionId { get; set; }
        public string BeneficiaryName { get; set; }
        public string Title { get; set; }
        public string VolunteerName { get; set; }
        public string Notes { get; set; }
        public string FileUrl { get; set; }

    }
}
