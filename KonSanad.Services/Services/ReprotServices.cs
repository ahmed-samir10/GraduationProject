using KonSanad.Core.Entity;
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
        //  Dashboard PDF
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

        // ================================================
        // Donors Last Month PDF
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


        // =====================================================
        //  Top Donors PDF
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
        //  Donations Between Dates PDF
        // =========================================================
        public async Task<byte[]> GetAllBeneficiariesAsync()
        {
            var beneficiaries = await _unitOfWork
                .Repository<Beneficiary, int>()
                .GetQueryable()
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

            return GenerateBeneficiaryPdf(
                "All Beneficiaries Report",
                beneficiaries);
        }
        public async Task<byte[]> GetTopBeneficiariesAsync(int top = 10)
        {
            var beneficiaries = await _unitOfWork
                .Repository<HelpOrder, int>()
                .GetQueryable()
                .Include(x => x.Beneficiary)
                .Where(x => x.HelpOrderStatus == RequestedStatus.Approved)
                .GroupBy(x => new
                {
                    x.Beneficiary.FullName,
                    x.Beneficiary.Email
                })
                .Select(g => new BeneficiaryReportDto
                {
                    Name = g.Key.FullName,
                    Email = g.Key.Email,
                    ApprovedRequests = g.Count()
                })
                .OrderByDescending(x => x.ApprovedRequests)
                .Take(top)
                .ToListAsync();

            return GenerateTopBeneficiariesPdf(
                "Top Beneficiaries Report",
                beneficiaries);
        }
        public async Task<byte[]> GetAllVolunteerAsync()
        {
            var volunteers = await _unitOfWork
                .Repository<Volunteer, int>()
                .GetQueryable()
                .Where(x => x.Status == RequestedStatus.Approved)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

            return GenerateVolunteerPdf(
                "All Volunteers Report",
                volunteers);
        }
        public async Task<byte[]> GetTopFastestVolunteersAsync(int top = 10)
        {
            var volunteers = await _unitOfWork
                .Repository<MissionVolunteers, int>()
                .GetQueryable()
                .Include(x => x.Volunteer)
                .Include(x => x.Mission)
                .Where(x =>
                    x.MissionStatus == VolunteerMissionStatus.Completed &&
                    x.UpdatedAt < x.Mission.EndDate)
                .GroupBy(x => new
                {
                    x.Volunteer.FullName,
                    x.Volunteer.Email
                })
                .Select(g => new TopVolunteerReportDto
                {
                    Name = g.Key.FullName,

                    Email = g.Key.Email,

                    CompletedMissions = g.Count(),

                    AverageDaysEarly =
                       (double) g.Average(x =>
                            EF.Functions.DateDiffDay(
                                x.UpdatedAt,
                                x.Mission.EndDate))
                })
                .OrderByDescending(x => x.AverageDaysEarly)
                .ThenByDescending(x => x.CompletedMissions)
                .Take(top)
                .ToListAsync();

            return GenerateTopVolunteersPdf(
                "Top Fastest Volunteers Report",
                volunteers);
        }
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
         
        
        
        //=========================================================
        // Active Campaigns
        // =========================================================

        public async Task<byte[]> GetActiveCampaignsAsync()
        {
            var campaigns = await _unitOfWork.Repository<CashDonation, int>()
                .GetQueryable()
                .Include(cd => cd.Campaign)
    .Where(cd => cd.CampaignId != null && cd.Campaign.CampaignStatus == CampaignStatus.NotCompleted)
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
        //  Completed Campaigns
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
        //  Expired Campaigns
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
        // Pending Requests PDF
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
        //  Completed Requests PDF
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


        private byte[] GenerateDonorsPdf(
       string title,
       List<DonorReportItem> donors)
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(20);

                    // لو عندك خط Cairo
                    // page.DefaultTextStyle(x => x.FontFamily("Cairo"));

                    // =========================================
                    // HEADER
                    // =========================================

                    page.Header()
                        .PaddingBottom(15)
                        .Column(col =>
                        {
                            col.Item()
                                .AlignCenter()
                                .Text("كن سند")
                                .FontSize(30)
                                .Bold()
                                .FontColor("#0F9D8A");

                            col.Item()
                                .AlignCenter()
                                .Text(title)
                                .FontSize(18)
                                .Bold()
                                .FontColor("#F4A300");

                            col.Item()
                                .PaddingTop(10)
                                .LineHorizontal(1)
                                .LineColor("#E5E7EB");
                        });

                    // =========================================
                    // CONTENT
                    // =========================================

                    page.Content()
                        .PaddingVertical(10)
                        .Column(col =>
                        {
                            // معلومات سريعة

                            col.Item()
                                .Background("#F8FAFC")
                                .Border(1)
                                .BorderColor("#E5E7EB")
                                .Padding(10)
                                .Row(row =>
                                {
                                    row.RelativeItem()
                                        .AlignCenter()
                                        .Text($"عدد المتبرعين: {donors.Count}");

                                    row.RelativeItem()
                                        .AlignCenter()
                                        .Text($"إجمالي التبرعات: {donors.Sum(x => x.Total):N0} جنيه");
                                });

                            col.Item().PaddingTop(15);

                            // =====================================
                            // TABLE
                            // =====================================

                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(3);
                                    columns.RelativeColumn(4);
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn(2);
                                });

                                // Header

                                table.Header(header =>
                                {
                                    header.Cell()
                                        .Element(CellStyle)
                                        .Background("#0F9D8A")
                                        .Text("اسم المتبرع")
                                        .FontColor(Colors.White)
                                        .Bold();

                                    header.Cell()
                                        .Element(CellStyle)
                                        .Background("#0F9D8A")
                                        .Text("البريد الإلكتروني")
                                        .FontColor(Colors.White)
                                        .Bold();

                                    header.Cell()
                                        .Element(CellStyle)
                                        .Background("#0F9D8A")
                                        .AlignCenter()
                                        .Text("إجمالي التبرعات")
                                        .FontColor(Colors.White)
                                        .Bold();

                                    header.Cell()
                                        .Element(CellStyle)
                                        .Background("#0F9D8A")
                                        .AlignCenter()
                                        .Text("عدد التبرعات")
                                        .FontColor(Colors.White)
                                        .Bold();
                                });

                                // Rows

                                for (int i = 0; i < donors.Count; i++)
                                {
                                    var d = donors[i];

                                    var bgColor ="#F8FAFC";

                                    table.Cell()
                                        .Element(CellStyle)
                                        .Background(bgColor)
                                        .Text(d.Name ?? "-");

                                    table.Cell()
                                        .Element(CellStyle)
                                        .Background(bgColor)
                                        .Text(d.Email ?? "-");

                                    table.Cell()
                                        .Element(CellStyle)
                                        .Background(bgColor)
                                        .AlignCenter()
                                        .Text($"{d.Total:N2} جنيه");

                                    table.Cell()
                                        .Element(CellStyle)
                                        .Background(bgColor)
                                        .AlignCenter()
                                        .Text(d.Count.ToString());
                                }
                            });
                        });

                    // =========================================
                    // FOOTER
                    // =========================================

                    page.Footer()
                        .Column(col =>
                        {
                            col.Item()
                                .LineHorizontal(1)
                                .LineColor("#E5E7EB");

                            col.Item()
                                .PaddingTop(5)
                                .AlignCenter()
                                .Text(text =>
                                {
                                    text.Span("تاريخ الإنشاء: ");
                                    text.Span(DateTime.Now.ToString("yyyy-MM-dd HH:mm"));
                                });
                        });
                });
            })
            .GeneratePdf();
        }

        private static IContainer CellStyle(IContainer container)
        {
            return container
                .BorderBottom(1)
                .BorderColor("#E5E7EB")
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
                    page.Size(PageSizes.A4);
                    page.Margin(20);

                    // =========================================
                    // HEADER
                    // =========================================

                    page.Header()
                        .PaddingBottom(15)
                        .Column(col =>
                        {
                            col.Item()
                                .AlignCenter()
                                .Text("كن سند")
                                .FontSize(30)
                                .Bold()
                                .FontColor("#0F9D8A");

                            col.Item()
                                .AlignCenter()
                                .Text(title)
                                .FontSize(18)
                                .Bold()
                                .FontColor("#F4A300");

                            col.Item()
                                .PaddingTop(10)
                                .LineHorizontal(1)
                                .LineColor("#E5E7EB");
                        });

                    // =========================================
                    // CONTENT
                    // =========================================

                    page.Content()
                        .PaddingVertical(10)
                        .Column(col =>
                        {
                            // Summary

                            col.Item()
                                .Background("#F8FAFC")
                                .Border(1)
                                .BorderColor("#E5E7EB")
                                .Padding(10)
                                .Row(row =>
                                {
                                    row.RelativeItem()
                                        .AlignCenter()
                                        .Text($"عدد الحملات: {campaigns.Count}");

                                    row.RelativeItem()
                                        .AlignCenter()
                                        .Text($"إجمالي الهدف: {campaigns.Sum(x => x.GoalCash):N0} جنيه");
                                });

                            col.Item().PaddingTop(15);

                            // Table

                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(4);
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn(2);
                                });

                                table.Header(header =>
                                {
                                    header.Cell()
                                        .Element(CellStyle)
                                        .Background("#0F9D8A")
                                        .Text("اسم الحملة")
                                        .FontColor(Colors.White)
                                        .Bold();

                                    header.Cell()
                                        .Element(CellStyle)
                                        .Background("#0F9D8A")
                                        .AlignCenter()
                                        .Text("الهدف")
                                        .FontColor(Colors.White)
                                        .Bold();

                                    header.Cell()
                                        .Element(CellStyle)
                                        .Background("#0F9D8A")
                                        .AlignCenter()
                                        .Text("المبلغ الحالي")
                                        .FontColor(Colors.White)
                                        .Bold();
                                });

                                for (int i = 0; i < campaigns.Count; i++)
                                {
                                    var c = campaigns[i];

                                    var bgColor = "#F8FAFC";

                                    table.Cell()
                                        .Element(CellStyle)
                                        .Background(bgColor)
                                        .Text(c.Title ?? "-");

                                    table.Cell()
                                        .Element(CellStyle)
                                        .Background(bgColor)
                                        .AlignCenter()
                                        .Text($"{c.GoalCash:N2} جنيه");

                                    table.Cell()
                                        .Element(CellStyle)
                                        .Background(bgColor)
                                        .AlignCenter()
                                        .Text($"{c.CurrentCash:N2} جنيه");
                                }
                            });
                        });

                    // =========================================
                    // FOOTER
                    // =========================================

                    page.Footer()
                        .Column(col =>
                        {
                            col.Item()
                                .LineHorizontal(1)
                                .LineColor("#E5E7EB");

                            col.Item()
                                .PaddingTop(5)
                                .AlignCenter()
                                .Text(text =>
                                {
                                    text.Span("تاريخ الإنشاء: ");
                                    text.Span(DateTime.Now.ToString("yyyy-MM-dd HH:mm"));
                                });
                        });
                });
            })
            .GeneratePdf();
        }
        private byte[] GenerateRequestsPdf(
            string title,
            List<HelpOrder> requests)
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(20);

                    // =========================================
                    // HEADER
                    // =========================================

                    page.Header()
                        .PaddingBottom(15)
                        .Column(col =>
                        {
                            col.Item()
                                .AlignCenter()
                                .Text("كن سند")
                                .FontSize(30)
                                .Bold()
                                .FontColor("#0F9D8A");

                            col.Item()
                                .AlignCenter()
                                .Text(title)
                                .FontSize(18)
                                .Bold()
                                .FontColor("#F4A300");

                            col.Item()
                                .PaddingTop(10)
                                .LineHorizontal(1)
                                .LineColor("#E5E7EB");
                        });

                    // =========================================
                    // CONTENT
                    // =========================================

                    page.Content()
                        .PaddingVertical(10)
                        .Column(col =>
                        {
                            // Summary

                            col.Item()
                                .Background("#F8FAFC")
                                .Border(1)
                                .BorderColor("#E5E7EB")
                                .Padding(10)
                                .Row(row =>
                                {
                                    row.RelativeItem()
                                        .AlignCenter()
                                        .Text($"عدد الطلبات: {requests.Count}");

                                    row.RelativeItem()
                                        .AlignCenter()
                                        .Text($"طلبات مكتملة: {requests.Count(x => x.HelpOrderStatus.ToString() == "Completed")}");
                                });

                            col.Item().PaddingTop(15);

                            // Table

                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(4);
                                    columns.RelativeColumn(2);
                                });

                                table.Header(header =>
                                {
                                    header.Cell()
                                        .Element(CellStyle)
                                        .Background("#0F9D8A")
                                        .Text("اسم المستفيد")
                                        .FontColor(Colors.White)
                                        .Bold();

                                    header.Cell()
                                        .Element(CellStyle)
                                        .Background("#0F9D8A")
                                        .AlignCenter()
                                        .Text("الحالة")
                                        .FontColor(Colors.White)
                                        .Bold();
                                });

                                for (int i = 0; i < requests.Count; i++)
                                {
                                    var r = requests[i];

                                    var bgColor = "#F8FAFC";

                                    table.Cell()
                                        .Element(CellStyle)
                                        .Background(bgColor)
                                        .Text(r?.Beneficiary?.FullName ?? "-");

                                    table.Cell()
                                        .Element(CellStyle)
                                        .Background(bgColor)
                                        .AlignCenter()
                                        .Text(r.HelpOrderStatus.ToString());
                                }
                            });
                        });

                    // =========================================
                    // FOOTER
                    // =========================================

                    page.Footer()
                        .Column(col =>
                        {
                            col.Item()
                                .LineHorizontal(1)
                                .LineColor("#E5E7EB");

                            col.Item()
                                .PaddingTop(5)
                                .AlignCenter()
                                .Text(text =>
                                {
                                    text.Span("تاريخ الإنشاء: ");
                                    text.Span(DateTime.Now.ToString("yyyy-MM-dd HH:mm"));
                                });
                        });
                });
            })
            .GeneratePdf();
        }

        private byte[] GenerateBeneficiaryPdf(
          string title,
          List<Beneficiary> beneficiaries)
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(20);

                    // =========================================
                    // HEADER
                    // =========================================

                    page.Header()
                        .PaddingBottom(15)
                        .Column(col =>
                        {
                            col.Item()
                                .AlignCenter()
                                .Text("كن سند")
                                .FontSize(30)
                                .Bold()
                                .FontColor("#0F9D8A");

                            col.Item()
                                .AlignCenter()
                                .Text(title)
                                .FontSize(18)
                                .Bold()
                                .FontColor("#F4A300");

                            col.Item()
                                .PaddingTop(10)
                                .LineHorizontal(1)
                                .LineColor("#E5E7EB");
                        });

                    // =========================================
                    // CONTENT
                    // =========================================

                    page.Content()
                        .PaddingVertical(10)
                        .Column(col =>
                        {
                            // Summary

                            col.Item()
                                .Background("#F8FAFC")
                                .Border(1)
                                .BorderColor("#E5E7EB")
                                .Padding(10)
                                .Row(row =>
                                {
                                    row.RelativeItem()
                                        .AlignCenter()
                                        .Text($" عدد المستفدين: {beneficiaries.Count}");

                                });

                            col.Item().PaddingTop(15);

                            // Table

                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(3); // Name
                                    columns.RelativeColumn(4); // Email
                                    columns.RelativeColumn(3); // Address
                                    columns.RelativeColumn(2); // Phone
                                });

                                table.Header(header =>
                                {
                                    header.Cell()
                                        .Element(CellStyle)
                                        .Background("#0F9D8A")
                                        .Text("اسم المستفيد")
                                        .FontColor(Colors.White)
                                        .Bold();

                                    header.Cell()
                                        .Element(CellStyle)
                                        .Background("#0F9D8A")
                                        .AlignCenter()
                                        .Text(" الايميل المسجل به")
                                        .FontColor(Colors.White)
                                        .Bold();

                                    header.Cell()
                                        .Element(CellStyle)
                                        .Background("#0F9D8A")
                                        .AlignCenter()
                                        .Text("العنوان")
                                        .FontColor(Colors.White)
                                        .Bold();

                                    header.Cell()
                                        .Element(CellStyle)
                                        .Background("#0F9D8A")
                                        .AlignCenter()
                                        .Text("رقم التلفون")
                                        .FontColor(Colors.White)
                                        .Bold();
                                });

                                for (int i = 0; i < beneficiaries.Count; i++)
                                {
                                    var r = beneficiaries[i];

                                    var bgColor = "#F8FAFC";

                                    table.Cell()
                                        .Element(CellStyle)
                                        .Background(bgColor)
                                        .Text(r?.FullName ?? "-");

                                    table.Cell()
                                        .Element(CellStyle)
                                        .Background(bgColor)
                                        .AlignCenter()
                                        .Text(r.Email.ToString());

                                    table.Cell()
                                        .Element(CellStyle)
                                        .Background(bgColor)
                                        .AlignCenter()
                                        .Text(r.Address.ToString());
                                    table.Cell()
                                        .Element(CellStyle)
                                        .Background(bgColor)
                                        .AlignCenter()
                                        .Text(r.Phone.ToString());
                                }
                            });
                        });

                    // =========================================
                    // FOOTER
                    // =========================================

                    page.Footer()
                        .Column(col =>
                        {
                            col.Item()
                                .LineHorizontal(1)
                                .LineColor("#E5E7EB");

                            col.Item()
                                .PaddingTop(5)
                                .AlignCenter()
                                .Text(text =>
                                {
                                    text.Span("تاريخ الإنشاء: ");
                                    text.Span(DateTime.Now.ToString("yyyy-MM-dd HH:mm"));
                                });
                        });
                });
            })
            .GeneratePdf();
        }
        private byte[] GenerateTopBeneficiariesPdf(
    string title,
    List<BeneficiaryReportDto> beneficiaries)
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(20);

                    page.Header()
                        .PaddingBottom(15)
                        .Column(col =>
                        {
                            col.Item()
                                .AlignCenter()
                                .Text("كن سند")
                                .FontSize(30)
                                .Bold()
                                .FontColor("#0F9D8A");

                            col.Item()
                                .AlignCenter()
                                .Text(title)
                                .FontSize(18)
                                .Bold()
                                .FontColor("#F4A300");

                            col.Item()
                                .PaddingTop(10)
                                .LineHorizontal(1)
                                .LineColor("#E5E7EB");
                        });

                    page.Content()
                        .Column(col =>
                        {
                            col.Item()
                                .Background("#F8FAFC")
                                .Border(1)
                                .BorderColor("#E5E7EB")
                                .Padding(10)
                                .AlignCenter()
                                .Text($"عدد المستفيدين: {beneficiaries.Count}");

                            col.Item().PaddingTop(15);

                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(4);
                                    columns.RelativeColumn(4);
                                    columns.RelativeColumn(2);
                                });

                                table.Header(header =>
                                {
                                    header.Cell()
                                        .Element(CellStyle)
                                        .Background("#0F9D8A")
                                        .Text("اسم المستفيد")
                                        .FontColor(Colors.White)
                                        .Bold();

                                    header.Cell()
                                        .Element(CellStyle)
                                        .Background("#0F9D8A")
                                        .Text("البريد الإلكتروني")
                                        .FontColor(Colors.White)
                                        .Bold();

                                    header.Cell()
                                        .Element(CellStyle)
                                        .Background("#0F9D8A")
                                        .AlignCenter()
                                        .Text("الطلبات")
                                        .FontColor(Colors.White)
                                        .Bold();
                                });

                                foreach (var b in beneficiaries)
                                {
                                    table.Cell()
                                        .Element(CellStyle)
                                        .Background("#F8FAFC")
                                        .Text(b.Name);

                                    table.Cell()
                                        .Element(CellStyle)
                                        .Background("#F8FAFC")
                                        .Text(b.Email);

                                    table.Cell()
                                        .Element(CellStyle)
                                        .Background("#F8FAFC")
                                        .AlignCenter()
                                        .Text(b.ApprovedRequests.ToString());
                                }
                            });
                        });

                    page.Footer()
                        .Column(col =>
                        {
                            col.Item()
                                .LineHorizontal(1)
                                .LineColor("#E5E7EB");

                            col.Item()
                                .PaddingTop(5)
                                .AlignCenter()
                                .Text($"تاريخ الإنشاء: {DateTime.Now:yyyy-MM-dd HH:mm}");
                        });
                });
            }).GeneratePdf();
        }
        private byte[] GenerateTopVolunteersPdf(
    string title,
    List<TopVolunteerReportDto> volunteers)
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(20);

                    // =========================================
                    // HEADER
                    // =========================================

                    page.Header()
                        .PaddingBottom(15)
                        .Column(col =>
                        {
                            col.Item()
                                .AlignCenter()
                                .Text("كن سند")
                                .FontSize(30)
                                .Bold()
                                .FontColor("#0F9D8A");

                            col.Item()
                                .AlignCenter()
                                .Text(title)
                                .FontSize(18)
                                .Bold()
                                .FontColor("#F4A300");

                            col.Item()
                                .PaddingTop(10)
                                .LineHorizontal(1)
                                .LineColor("#E5E7EB");
                        });

                    // =========================================
                    // CONTENT
                    // =========================================

                    page.Content()
                        .PaddingVertical(10)
                        .Column(col =>
                        {
                            // Summary

                            col.Item()
                                .Background("#F8FAFC")
                                .Border(1)
                                .BorderColor("#E5E7EB")
                                .Padding(10)
                                .Row(row =>
                                {
                                    row.RelativeItem()
                                        .AlignCenter()
                                        .Text($"عدد المتطوعين: {volunteers.Count}");

                                    row.RelativeItem()
                                        .AlignCenter()
                                        .Text($"إجمالي المهام المكتملة: {volunteers.Sum(x => x.CompletedMissions)}");
                                });

                            col.Item().PaddingTop(15);

                            // =====================================
                            // TABLE
                            // =====================================

                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(3); // Name
                                    columns.RelativeColumn(4); // Email
                                    columns.RelativeColumn(2); // Missions
                                    columns.RelativeColumn(2); // Early Days
                                });

                                table.Header(header =>
                                {
                                    header.Cell()
                                        .Element(CellStyle)
                                        .Background("#0F9D8A")
                                        .Text("اسم المتطوع")
                                        .FontColor(Colors.White)
                                        .Bold();

                                    header.Cell()
                                        .Element(CellStyle)
                                        .Background("#0F9D8A")
                                        .Text("البريد الإلكتروني")
                                        .FontColor(Colors.White)
                                        .Bold();

                                    header.Cell()
                                        .Element(CellStyle)
                                        .Background("#0F9D8A")
                                        .AlignCenter()
                                        .Text("المهام")
                                        .FontColor(Colors.White)
                                        .Bold();

                                    header.Cell()
                                        .Element(CellStyle)
                                        .Background("#0F9D8A")
                                        .AlignCenter()
                                        .Text("متوسط الأيام المبكرة")
                                        .FontColor(Colors.White)
                                        .Bold();
                                });

                                foreach (var v in volunteers)
                                {
                                    var bgColor = "#F8FAFC";

                                    table.Cell()
                                        .Element(CellStyle)
                                        .Background(bgColor)
                                        .Text(v.Name ?? "-");

                                    table.Cell()
                                        .Element(CellStyle)
                                        .Background(bgColor)
                                        .Text(v.Email ?? "-");

                                    table.Cell()
                                        .Element(CellStyle)
                                        .Background(bgColor)
                                        .AlignCenter()
                                        .Text(v.CompletedMissions.ToString());

                                    table.Cell()
                                        .Element(CellStyle)
                                        .Background(bgColor)
                                        .AlignCenter()
                                        .Text($"{v.AverageDaysEarly:N1} يوم");
                                }
                            });
                        });

                    // =========================================
                    // FOOTER
                    // =========================================

                    page.Footer()
                        .Column(col =>
                        {
                            col.Item()
                                .LineHorizontal(1)
                                .LineColor("#E5E7EB");

                            col.Item()
                                .PaddingTop(5)
                                .AlignCenter()
                                .Text(text =>
                                {
                                    text.Span("تاريخ الإنشاء: ");
                                    text.Span(DateTime.Now.ToString("yyyy-MM-dd HH:mm"));
                                });
                        });
                });
            })
            .GeneratePdf();
        }
        private byte[] GenerateVolunteerPdf(
        string title,
        List<Volunteer> beneficiaries)
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(20);

                    // =========================================
                    // HEADER
                    // =========================================

                    page.Header()
                        .PaddingBottom(15)
                        .Column(col =>
                        {
                            col.Item()
                                .AlignCenter()
                                .Text("كن سند")
                                .FontSize(30)
                                .Bold()
                                .FontColor("#0F9D8A");

                            col.Item()
                                .AlignCenter()
                                .Text(title)
                                .FontSize(18)
                                .Bold()
                                .FontColor("#F4A300");

                            col.Item()
                                .PaddingTop(10)
                                .LineHorizontal(1)
                                .LineColor("#E5E7EB");
                        });

                    // =========================================
                    // CONTENT
                    // =========================================

                    page.Content()
                        .PaddingVertical(10)
                        .Column(col =>
                        {
                            // Summary

                            col.Item()
                                .Background("#F8FAFC")
                                .Border(1)
                                .BorderColor("#E5E7EB")
                                .Padding(10)
                                .Row(row =>
                                {
                                    row.RelativeItem()
                                        .AlignCenter()
                                        .Text($" عدد المتطوعين: {beneficiaries.Count}");

                                });

                            col.Item().PaddingTop(15);

                            // Table

                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(3); // Name
                                    columns.RelativeColumn(4); // Email
                                    columns.RelativeColumn(3); // Address
                                    columns.RelativeColumn(2); // Phone
                                });

                                table.Header(header =>
                                {
                                    header.Cell()
                                        .Element(CellStyle)
                                        .Background("#0F9D8A")
                                        .Text("اسم المتطوع")
                                        .FontColor(Colors.White)
                                        .Bold();

                                    header.Cell()
                                        .Element(CellStyle)
                                        .Background("#0F9D8A")
                                        .AlignCenter()
                                        .Text(" الايميل المسجل به")
                                        .FontColor(Colors.White)
                                        .Bold();

                                    header.Cell()
                                        .Element(CellStyle)
                                        .Background("#0F9D8A")
                                        .AlignCenter()
                                        .Text("العنوان")
                                        .FontColor(Colors.White)
                                        .Bold();

                                    header.Cell()
                                        .Element(CellStyle)
                                        .Background("#0F9D8A")
                                        .AlignCenter()
                                        .Text("رقم التلفون")
                                        .FontColor(Colors.White)
                                        .Bold();
                                });

                                for (int i = 0; i < beneficiaries.Count; i++)
                                {
                                    var r = beneficiaries[i];

                                    var bgColor = "#F8FAFC";

                                    table.Cell()
                                        .Element(CellStyle)
                                        .Background(bgColor)
                                        .Text(r?.FullName ?? "-");

                                    table.Cell()
                                        .Element(CellStyle)
                                        .Background(bgColor)
                                        .AlignCenter()
                                        .Text(r.Email.ToString());

                                    table.Cell()
                                        .Element(CellStyle)
                                        .Background(bgColor)
                                        .AlignCenter()
                                        .Text(r.Address.ToString());
                                    table.Cell()
                                        .Element(CellStyle)
                                        .Background(bgColor)
                                        .AlignCenter()
                                        .Text(r.Phone.ToString());
                                }
                            });
                        });

                    // =========================================
                    // FOOTER
                    // =========================================

                    page.Footer()
                        .Column(col =>
                        {
                            col.Item()
                                .LineHorizontal(1)
                                .LineColor("#E5E7EB");

                            col.Item()
                                .PaddingTop(5)
                                .AlignCenter()
                                .Text(text =>
                                {
                                    text.Span("تاريخ الإنشاء: ");
                                    text.Span(DateTime.Now.ToString("yyyy-MM-dd HH:mm"));
                                });
                        });
                });
            })
            .GeneratePdf();
        }

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
