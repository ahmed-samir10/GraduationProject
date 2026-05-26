using KonSanad.Core.Entity.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
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
                  .Include(x => x.CashDonation)
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
                    Total = g.Sum(x =>
                  x.CashDonation != null
                      ? x.CashDonation.Amount
                      : 0),

                    // عدد عمليات التبرع


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
                .Include(x => x.CashDonation)
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
                    Total =g.Sum(x => x.CashDonation != null ? x.CashDonation.Amount : 0),

                    Count = g.Count()
                })
                 .OrderByDescending(x => x.Total)   // 🔥 الترتيب الصح
        .ThenByDescending(x => x.Count)
        .Take(top)
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
                .Include(x => x.CashDonation)
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
                    Total = g.Sum(x => x.CashDonation != null ? x.CashDonation.Amount : 0),

                    Count = g.Count()
                })
                .ToListAsync();

            return GenerateDonorsPdf("Donations Between Dates", donors);
        }

        // =========================================================
        // 🏕 Active Campaigns PDF
        // =========================================================

        // ✅ Active Campaigns
        // =========================================================

        public async Task<byte[]> GetActiveCampaignsAsync()
        {
            var campaigns = await _unitOfWork.Repository<CashDonation, int>()
                .GetQueryable()
                .Include(cd => cd.Campaign)
    .Where(cd => cd.CampaignId != null)
    .GroupBy(cd => cd.CampaignId)
    .Select(g => new CampaignReportDto
    {
        Title = g.First().Campaign!.Title,
        GoalCash = g.First().Campaign!.GoalCash ?? 0,
        CurrentCash = g.Sum(x => x.Amount),
        Date = g.First().Campaign!.CreatedAt
    })
    .ToListAsync();

            return GenerateCampaignsPdf("Active Campaigns", campaigns);
        }

        // =========================================================
        // ⛔ Completed Campaigns
        // =========================================================
     public async Task<byte[]> GetCompletedCampaignsAsync()
        {

            var campaigns = await _unitOfWork.Repository<CashDonation, int>()
                .GetQueryable()
                .Include(cd => cd.Campaign)
    .Where(cd => cd.CampaignId != null && cd.Campaign.CampaignStatus == CampaignStatus.Completed)
    .GroupBy(cd => cd.CampaignId)
    .Select(g => new CampaignReportDto
    {
        Title = g.First().Campaign!.Title,
        GoalCash = g.First().Campaign!.GoalCash ?? 0,
        CurrentCash = g.Sum(x => x.Amount),
        Date = g.First().Campaign!.CreatedAt
    })
    .ToListAsync();

            return GenerateCampaignsPdf("Completed Campaigns", campaigns);
        }

        // =========================================================
        // ⏰ Expired Campaigns
        // =========================================================

        public async Task<byte[]> GetExpiredCampaignsAsync()
        {

            var campaigns = await _unitOfWork.Repository<CashDonation, int>()
                .GetQueryable()
                .Include(cd => cd.Campaign)
    .Where(x => x.Campaign.EndDate < DateTime.Now)
    .GroupBy(cd => cd.CampaignId)
    .Select(g => new CampaignReportDto
    {
        Title = g.First().Campaign!.Title,
        GoalCash = g.First().Campaign!.GoalCash ?? 0,
        CurrentCash = g.Sum(x => x.Amount),
        Date = g.First().Campaign!.CreatedAt
    })
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
            var requests = await _unitOfWork
                .Repository<HelpOrder, int>()
                .GetQueryable()

                .Include(x => x.Beneficiary)

                .Where(x =>
                    x.HelpOrderStatus ==
                    Core.Entity.Enums.RequestedStatus.Pending)

                .OrderByDescending(x => x.CreatedAt)

                .ToListAsync();

            return GenerateRequestsPdf(
                "Pending Requests",
                requests);
        }

        // =========================================================
        // ✅ Completed Requests PDF
        // =========================================================

        public async Task<byte[]> GetCompletedRequestsAsync()
        {
            var requests = await _unitOfWork.Repository<HelpOrder, int>()
                .GetQueryable()
                .Include(x => x.Beneficiary)
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

                    // =========================================
                    // Header
                    // =========================================

                    page.Header()
                        .Text(title)
                        .FontSize(22)
                        .Bold()
                        .FontColor(Colors.Blue.Darken2);

                    // =========================================
                    // Content
                    // =========================================

                    page.Content().PaddingVertical(10).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(4);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                        });

                        // =====================================
                        // Header
                        // =====================================

                        table.Header(header =>
                        {
                            header.Cell().Element(CellStyle)
                                .Background(Colors.Grey.Lighten2)
                                .Text("Name").Bold();

                            header.Cell().Element(CellStyle)
                                .Background(Colors.Grey.Lighten2)
                                .Text("Email").Bold();

                            header.Cell().Element(CellStyle)
                                .Background(Colors.Grey.Lighten2)
                                .AlignRight()
                                .Text("Total").Bold();

                            header.Cell().Element(CellStyle)
                                .Background(Colors.Grey.Lighten2)
                                .AlignCenter()
                                .Text("Count").Bold();
                        });

                        // =====================================
                        // Rows
                        // =====================================

                        foreach (var d in donors)
                        {
                            table.Cell().Element(CellStyle)
                                .Text(d.Name ?? "-");

                            table.Cell().Element(CellStyle)
                                .Text(d.Email ?? "-");

                            table.Cell().Element(CellStyle)
                                .AlignRight()
                                .Text($"{d.Total:N2} EGP");

                            table.Cell().Element(CellStyle)
                                .AlignCenter()
                                .Text(d.Count.ToString());
                        }
                    });

                    // =========================================
                    // Footer
                    // =========================================

                    page.Footer()
                        .AlignCenter()
                        .Text(x =>
                        {
                            x.Span("Generated At: ");
                            x.Span(DateTime.Now.ToString("yyyy-MM-dd hh:mm tt"));
                        });

                });
            }).GeneratePdf();
        }

        private IContainer CellStyle(IContainer container)
        {
            return container
                .BorderBottom(1)
                .BorderColor(Colors.Grey.Lighten2)
                .PaddingVertical(8)
                .PaddingHorizontal(5);
        }

        private byte[] GenerateCampaignsPdf(
     string title,
     List<CampaignReportDto> campaigns)
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(20);

                    // =====================================
                    // Header
                    // =====================================
                    page.Header().Column(column =>
                    {
                        column.Item()
                            .Text(title)
                            .FontSize(24)
                            .Bold()
                            .FontColor(Colors.Green.Darken2);

                        column.Item()
                            .Text($"Generated At: {DateTime.Now:yyyy-MM-dd hh:mm tt}")
                            .FontSize(10)
                            .FontColor(Colors.Grey.Darken1);
                    });

                    // =====================================
                    // Content
                    // =====================================
                    page.Content()
                        .PaddingTop(15)
                        .Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(4);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(2);
                            });

                            // HEADER
                            table.Header(header =>
                            {
                                header.Cell()
                                    .Element(CampaignHeaderStyle)
                                    .Text("Title");

                                header.Cell()
                                    .Element(CampaignHeaderStyle)
                                    .AlignCenter()
                                    .Text("Goal");

                                header.Cell()
                                    .Element(CampaignHeaderStyle)
                                    .AlignCenter()
                                    .Text("Current");
                            });

                            // ROWS
                            int index = 0;

                            foreach (var c in campaigns)
                            {
                                var background = index % 2 == 0
                                    ? Colors.White
                                    : Colors.Grey.Lighten4;

                                table.Cell()
                                    .Element(x =>
                                        CampaignCellStyle(x)
                                        .Background(background))
                                    .Text(c.Title ?? "N/A");

                                table.Cell()
                                    .Element(x =>
                                        CampaignCellStyle(x)
                                        .Background(background))
                                    .AlignCenter()
                                    .Text($"{c.GoalCash:N2} EGP");

                                table.Cell()
                                    .Element(x =>
                                        CampaignCellStyle(x)
                                        .Background(background))
                                    .AlignCenter()
                                    .Text($"{c.CurrentCash:N2} EGP");

                                index++;
                            }
                        });

                    // =====================================
                    // Footer
                    // =====================================
                    page.Footer()
                        .AlignCenter()
                        .Text("KonSanad System Reports");
                });
            }).GeneratePdf();
        }
        // =========================================================
        // 🎨 Styles
        // =========================================================

        private IContainer CampaignHeaderStyle(IContainer container)
        {
            return container
                .Background(Colors.Grey.Lighten2)
                .Padding(8)
                .BorderBottom(1)
                .BorderColor(Colors.Grey.Medium)
                .DefaultTextStyle(x => x.Bold());
        }

        private IContainer CampaignCellStyle(IContainer container)
        {
            return container
                .Padding(8)
                .BorderBottom(1)
                .BorderColor(Colors.Grey.Lighten2);
        }

        private byte[] GenerateRequestsPdf(
         string title,
         List<HelpOrder> requests)
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(20);

                    // =====================================
                    // Header
                    // =====================================

                    page.Header().Column(column =>
                    {
                        column.Item()
                            .Text(title)
                            .FontSize(24)
                            .Bold()
                            .FontColor(Colors.Red.Darken2);

                        column.Item()
                            .Text($"Generated At: {DateTime.Now:yyyy-MM-dd hh:mm tt}")
                            .FontSize(10)
                            .FontColor(Colors.Grey.Darken1);
                    });

                    // =====================================
                    // Table
                    // =====================================

                    page.Content().PaddingTop(15).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(4);
                            columns.RelativeColumn(2);
                        });

                        // =================================
                        // Header
                        // =================================

                        table.Header(header =>
                        {
                            header.Cell()
                                .Element(HeaderCellStyle)
                                .Text("User");

                            header.Cell()
                                .Element(HeaderCellStyle)
                                .AlignCenter()
                                .Text("Status");
                        });

                        // =================================
                        // Rows
                        // =================================

                        int index = 0;

                        foreach (var r in requests)
                        {
                            var background = index % 2 == 0
                                ? Colors.White
                                : Colors.Grey.Lighten4;

                            table.Cell()
                                .Element(c =>
                                    CellStyle(c).Background(background))
                                .Text(r?.Beneficiary?.FullName ?? "N/A");

                            table.Cell()
                                .Element(c =>
                                    CellStyle(c).Background(background))
                                .AlignCenter()
                                .Text(r.HelpOrderStatus.ToString());

                            index++;
                        }
                    });

                    // =====================================
                    // Footer
                    // =====================================

                    page.Footer()
                        .AlignCenter()
                        .Text(x =>
                        {
                            x.Span("KonSanad System Reports");
                        });
                });
            }).GeneratePdf();
        }

        private IContainer HeaderCellStyle(IContainer container)
        {
            return container
                .Background(Colors.Grey.Lighten2)
                .Padding(8)
                .BorderBottom(1)
                .BorderColor(Colors.Grey.Medium)
                .DefaultTextStyle(x => x.Bold());
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
