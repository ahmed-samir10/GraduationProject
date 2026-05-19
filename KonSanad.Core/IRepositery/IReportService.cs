using KonSanad.Core.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KonSanad.Core.IRepositery
{
    public interface IReportService
    {
        // Dashboard
        Task<DashboardDto> GetDashboardAsync();

        // Donors Reports
        Task<byte[]> GetDonorsLastMonthAsync();

        Task<byte[]> GetTopDonorsAsync(int top = 10);

        Task<byte[]> GetDonationsBetweenDatesAsync(
            DateTime from,
            DateTime to);

        // Campaign Reports
        Task<byte[]> GetActiveCampaignsAsync();

        Task<byte[]> GetExpiredCampaignsAsync();



        // Help Requests Reports
        Task<byte[]> GetPendingRequestsAsync();

        Task<byte[]> GetCompletedRequestsAsync();

 
        // Export
        Task<byte[]> ExportDonorsPdfAsync();

    }
}
