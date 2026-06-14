using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KonSanad.Core.Dtos
{
    public class BeneficiaryReportDto
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public int ApprovedRequests { get; set; }
    }
    public class TopVolunteerReportDto
    {
        public string Name { get; set; }

        public string Email { get; set; }

        public int CompletedMissions { get; set; }

        public double AverageDaysEarly { get; set; }
    }
}
