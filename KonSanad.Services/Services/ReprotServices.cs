using Microsoft.AspNetCore.Http;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KonSanad.Services.Services
{
    public class ReportService : IReportService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ReportService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            QuestPDF.Settings.License = LicenseType.Community;
        }

        // =========================================================
        // 📊 Dashboard PDF
        // =========================================================

        public async Task<DashboardDto> GetDashboardAsync()
        {
            var dashboardDto = new DashboardDto();

            // =====================================================
            // 💰 Total Cash Donations
            // =====================================================

            dashboardDto.TotalCashDonations = await _unitOfWork
                .Repository<CashDonation, int>()
                .GetQueryable()
               
                .SumAsync(x => (decimal?)x.Amount) ?? 0;

            // =====================================================
            // 🎁 Total In-Kind Donations
            // =====================================================

            dashboardDto.TotalInKindDonations = await _unitOfWork
                .Repository<OrderItem, int>()
                .GetQueryable()
                .CountAsync();

            // =====================================================
            // 👥 Total Donors
            // =====================================================

            dashboardDto.TotalDonors = await _unitOfWork
                .Repository<Donor, int>()
                .GetQueryable()
                .CountAsync();

            // =====================================================
            // 🙋 Total Volunteers
            // =====================================================

            dashboardDto.TotalVolunteers = await _unitOfWork
                .Repository<Volunteer, int>()
                .GetQueryable()
                .CountAsync();

            // =====================================================
            // 🏕 Active Campaigns
            // =====================================================

            dashboardDto.ActiveCampaigns = await _unitOfWork
                .Repository<Campaign, int>()
                .GetQueryable()
                .CountAsync(x =>
                    x.CampaignStatus ==
                    Core.Entity.Enums.CampaignStatus.NotCompleted);

            // =====================================================
            // ⏳ Pending Requests
            // =====================================================

            dashboardDto.PendingRequests = await _unitOfWork
                .Repository<HelpOrder, int>()
                .GetQueryable()
                .CountAsync(x =>
                    x.HelpOrderStatus ==
                    Core.Entity.Enums.RequestedStatus.Pending);

            // =====================================================
            // ✅ Completed Requests
            // =====================================================

            dashboardDto.CompletedRequests = await _unitOfWork
                .Repository<HelpOrder, int>()
                .GetQueryable()
                .CountAsync(x =>
                    x.HelpOrderStatus ==
                    Core.Entity.Enums.RequestedStatus.Approved);

            // =====================================================
            // 👨‍👩‍👧 Total Beneficiaries
            // =====================================================

            dashboardDto.TotalBeneficiaries = await _unitOfWork
                .Repository<HelpOrder, int>()
                .GetQueryable()
                .CountAsync(x =>
                    x.HelpOrderStatus ==
                    Core.Entity.Enums.RequestedStatus.Approved);

            // =====================================================
            // 📅 This Month Donations
            // =====================================================

            var currentMonth = DateTime.Now.Month;
            var currentYear = DateTime.Now.Year;

            dashboardDto.ThisMonthDonations = await _unitOfWork
                .Repository<CashDonation, int>()
                .GetQueryable()
                .Where(x =>
                    x.CreatedAt.Month == currentMonth &&
                    x.CreatedAt.Year == currentYear)
                .SumAsync(x => (decimal?)x.Amount) ?? 0;


            return dashboardDto;
        }

        // =========================================================
        // 👥 Donors Last Month PDF
        // =========================================================

        // =====================================================
        // 👥 Donors Last Month PDF
        // =====================================================

        public async Task<byte[]> GetDonorsLastMonthAsync()
        {
            var lastMonth = DateTime.Now.AddMonths(-1);

            var donors = await _unitOfWork
                .Repository<DonationOrder, int>()
                .GetQueryable()
                .Include(x => x.Donor)
                .Where(x => x.CreatedAt >= lastMonth)
                .GroupBy(x => new
                {
                    x.Donor.FullName,
                    x.Donor.Email
                })
                .Select(g => new DonorReportItem
                {
                    Name = g.Key.FullName,

                    Email = g.Key.Email,

                    // غيرها لو عندك Amount حقيقي
                    Total = 0,

                    Count = g.Count()
                })
                .ToListAsync();

            return GenerateDonorsPdf(
                "Donors Last Month",
                donors);
        }

        // =========================================================
        // 🏆 Top Donors PDF
        // =========================================================

        // =====================================================
        // 🏆 Top Donors PDF
        // =====================================================

        public async Task<byte[]> GetTopDonorsAsync(int top = 10)
        {
            var donors = await _unitOfWork
                .Repository<DonationOrder, int>()
                .GetQueryable()
                .Include(x => x.Donor)
                .GroupBy(x => new
                {
                    x.Donor.FullName,
                    x.Donor.Email
                })
                .Select(g => new DonorReportItem
                {
                    Name = g.Key.FullName,

                    Email = g.Key.Email,

                    // غيرها لو عندك Amount
                    Total = 0,

                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .Take(top)
                .ToListAsync();

            return GenerateDonorsPdf(
                "Top Donors Report",
                donors);
        }

        // =========================================================
        // 📅 Donations Between Dates PDF
        // =========================================================

        public async Task<byte[]> GetDonationsBetweenDatesAsync(DateTime from, DateTime to)
        {
            var donors = await _unitOfWork.Repository<DonationOrder, int>()
                .GetQueryable()
                .Where(x => x.CreatedAt >= from && x.CreatedAt <= to)
                .GroupBy(x => new
                {
                    x.Donor.FullName,
                    x.Donor.Email
                })
                .Select(g => new DonorReportItem
                {
                    Name = g.Key.FullName,
                    Email = g.Key.Email,
                   Total= 0,
                    Count = g.Count()
                })
                .ToListAsync();

            return GenerateDonorsPdf("Donations Between Dates", donors);
        }

        // =========================================================
        // 🏕 Active Campaigns PDF
        // =========================================================

        public async Task<byte[]> GetActiveCampaignsAsync()
        {
            var campaigns = await _unitOfWork.Repository<Campaign, int>()
                .GetQueryable()
                .Where(x => x.CampaignStatus == Core.Entity.Enums.CampaignStatus.NotCompleted)
                .ToListAsync();

            return GenerateCampaignsPdf("Active Campaigns", campaigns);
        }

        // =========================================================
        // 🏕 Expired Campaigns PDF
        // =========================================================

        public async Task<byte[]> GetExpiredCampaignsAsync()
        {
            var campaigns = await _unitOfWork.Repository<Campaign, int>()
                .GetQueryable()
                .Where(x => x.EndDate < DateTime.Now)
                .ToListAsync();

            return GenerateCampaignsPdf("Expired Campaigns", campaigns);
        }

        // =========================================================
        // 📈 Campaign Performance PDF
        // =========================================================

    
        // =========================================================
        // 🆘 Pending Requests PDF
        // =========================================================

        public async Task<byte[]> GetPendingRequestsAsync()
        {
            var requests = await _unitOfWork.Repository<HelpOrder, int>()
                .GetQueryable()
                .Where(x => x.HelpOrderStatus == Core.Entity.Enums.RequestedStatus.Pending)
                .ToListAsync();

            return GenerateRequestsPdf("Pending Requests", requests);
        }

        // =========================================================
        // ✅ Completed Requests PDF
        // =========================================================

        public async Task<byte[]> GetCompletedRequestsAsync()
        {
            var requests = await _unitOfWork.Repository<HelpOrder, int>()
                .GetQueryable()
                .Where(x => x.HelpOrderStatus == Core.Entity.Enums.RequestedStatus.Approved)
                .ToListAsync();

            return GenerateRequestsPdf("Completed Requests", requests);
        }



        // =========================================================
        // 🔥 Helper Methods
        // =========================================================
        // =====================================================
        // 🔥 Generate Donors PDF
        // =====================================================

        private byte[] GenerateDonorsPdf(
            string title,
            List<DonorReportItem> donors)
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(20);

                    // =================================================
                    // Header
                    // =================================================

                    page.Header()
                        .Text(title)
                        .FontSize(22)
                        .Bold();

                    // =================================================
                    // Table
                    // =================================================

                    page.Content().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(4);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                        });

                        // =============================================
                        // Header Row
                        // =============================================

                        table.Header(header =>
                        {
                            header.Cell().Text("Name").Bold();

                            header.Cell().Text("Email").Bold();

                            header.Cell().Text("Total").Bold();

                            header.Cell().Text("Count").Bold();
                        });

                        // =============================================
                        // Data
                        // =============================================

                        foreach (var d in donors)
                        {
                            table.Cell()
                                .Text(d.Name ?? "");

                            table.Cell()
                                .Text(d.Email ?? "");

                            table.Cell()
                                .Text($"{d.Total} EGP");

                            table.Cell()
                                .Text(d.Count.ToString());
                        }
                    });
                });
            }).GeneratePdf();
        }
        private byte[] GenerateCampaignsPdf(string title, List<Campaign> campaigns)
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(20);

                    page.Header()
                        .Text(title)
                        .FontSize(22)
                        .Bold();

                    page.Content().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                        });

                        table.Header(header =>
                        {
                            header.Cell().Text("Title");
                            header.Cell().Text("Goal");
                            header.Cell().Text("Current");
                        });

                        foreach (var c in campaigns)
                        {
                            table.Cell().Text(c.Title);
                            table.Cell().Text(c.GoalCash.ToString());
                            
                        }
                    });
                });
            }).GeneratePdf();
        }

        private byte[] GenerateRequestsPdf(string title, List<HelpOrder> requests)
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(20);

                    page.Header()
                        .Text(title)
                        .FontSize(22)
                        .Bold();

                    page.Content().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                        });

                        table.Header(header =>
                        {
                            header.Cell().Text("User");
                         
                            header.Cell().Text("Status");
                        });

                        foreach (var r in requests)
                        {
                            table.Cell().Text(r.Beneficiary.FullName);
                       
                            table.Cell().Text(r.HelpOrderStatus);
                        }
                    });
                });
            }).GeneratePdf();
        }

        // =========================================================
        // ❌ Excel not needed now
        // =========================================================

        public Task<byte[]> ExportDonorsPdfAsync()
        {
            throw new NotImplementedException();
        }

        public Task<byte[]> ExportTopDonorsExcelAsync()
        {
            throw new NotImplementedException();
        }
    }
}
