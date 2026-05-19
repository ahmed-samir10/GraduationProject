using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KonSanad.Core.Dtos
{
    public class DashboardDto
    {
        public decimal TotalCashDonations { get; set; }

        public decimal TotalInKindDonations { get; set; }

        public int TotalDonors { get; set; }

        public int TotalVolunteers { get; set; }

        public int ActiveCampaigns { get; set; }

        public int PendingRequests { get; set; }

        public int CompletedRequests { get; set; }

        public int TotalBeneficiaries { get; set; }

        public decimal ThisMonthDonations { get; set; }

    }
}
