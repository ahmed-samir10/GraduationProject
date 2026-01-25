using AutoMapper;
using KonSanad.Core.Dtos;
using KonSanad.Core.Entity.Identity;
using KonSanad.Core.IRepositery;
using KonSanad.Repository.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KonSanad.Services.Services
{
    public class DonorServices : IDonorServices
    {
       // private readonly IDonorServices _orderService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly UserManager<AppUser> _userManager;

        public DonorServices(
         
            IUnitOfWork unitOfWork,
            IMapper mapper, UserManager<AppUser> userManager)
        {
         
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _userManager = userManager;

        }


        // ---------------------------------------------------------------------------
        // 2) Get All Low-Stock Supplies
        // ---------------------------------------------------------------------------
        public async Task<List<SupplyDto>> GetLowStockSuppliesAsync()
        {

            
            var supplies = await _unitOfWork.Repository<Supply, int>()
                .FindAllAsync(s => s.StockQuantity <= s.MinimumLevel);

            return supplies.Select(s => new SupplyDto
            {
                Id = s.Id,
                Name = s.Name,
               Quantity = s.StockQuantity ?? 0,
                Unit = s.Unit
            }).ToList();
        }

      
        public async Task<DonationResponseCashDto> CreateDonationCashAsync(
           CreateDonationCashRequestDto request, int donorId)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var donationOrder = new DonationOrder
            {
                DonorId = donorId,
                CampaignId = request.CampaignId,
                ManagerId = 1, // Default manager assignment
                OrderDate = DateTime.Now,
                CreatedAt = DateTime.Now,
                DonationCategoryStatus = "معلقه",
                Status = "قيد المراجعة",


            };

            await _unitOfWork.Repository<DonationOrder, int>().AddAsync(donationOrder);
            await _unitOfWork.Complete();


            int? cashReceiptId = null;
           

            // Step 2: Cash donation
            if (request.CashAmount.HasValue && request.CashAmount > 0)
            {
                donationOrder.CashAmount = request.CashAmount.Value;
                _unitOfWork.Repository<DonationOrder, int>().Update(donationOrder);
                await _unitOfWork.Complete();

                var cash = new CashDonation
                {

                    DonationOrderId = donationOrder.Id,
                    Amount = request.CashAmount.Value
                };

                await _unitOfWork.Repository<CashDonation, int>().AddAsync(cash);
                await _unitOfWork.Complete();
                var receipt = new Receipt
                {
                    DonationOrderId = donationOrder.Id,
                    CashDonationId = cash.Id,

                    DonorId = donorId,
                    ManagerId = 1
                };

                // Save
                await _unitOfWork.Repository<Receipt, int>().AddAsync(receipt);

                await _unitOfWork.Complete();

                cashReceiptId = receipt.Id;
            }

            
            await _unitOfWork.Complete();

            // Step 4: Response

            
                return new DonationResponseCashDto
                {

                    CashAmount = request.CashAmount,

                    CashReceiptId = cashReceiptId,
                    Message = "تم تسجيل التبرع بنجاح ✔"
                };

           



        }
        // get myrequst 

        // ---------------------------------------------------------------------------
        // New: Get all DonationOrders made by the donor (cash and/or items)
        // ---------------------------------------------------------------------------
        public async Task<IEnumerable<DonationItemDto>> GetMyDonationsAsync(string userId)
        {
            // 1) Get donor by AppUserId
            var donor = await _unitOfWork.Repository<Donor, int>()
                .FirstOrDefaultAsync(d => d.AppUserId == userId);

            if (donor == null)
                throw new Exception("Donor not found");

            // 2) Query DonationOrders for this donor, include items, supplies, cash donation and receipts
            var donations = await _unitOfWork.Repository<DonationOrder, int>()
                .FindAllAsync(
                    d => d.DonorId == donor.Id,
                    include: q => q
                        .Include(d => d.OrderItems)
                            .ThenInclude(oi => oi.Supply)
                        .Include(d => d.CashDonation)
                        .Include(d => d.Receipts)
                );

            // 3) Map to DTO
            var result = donations.Select(d => new DonationItemDto
            {
                Id = d.Id,
                OrderDate = d.OrderDate,
                CreatedAt = d.CreatedAt,
                CashAmount = d.CashAmount,
                Status = d.Status,
                Items = d.OrderItems?.Select(oi => new DonationItemDto
                {
                    SupplyId = oi.SupplyId ?? 0,
                    Quantity = oi.Quantity,
                    CustomName = oi.CustomSupplyName,
                    ImageFile = oi.ImageUrl,
                    Unit = oi.Unit
                }).ToList() ?? new List<DonationItemDto>(),
                ReceiptIds = d.Receipts?.Select(r => r.Id).ToList() ?? new List<int>()
            }).ToList();

            return result;
        }
        public async Task<IEnumerable<CampaignDto>> GetCampaignsDonatedByAsync(string userId)
        {
            // 1) Get donor by AppUserId
            var donor = await _unitOfWork.Repository<Donor, int>()
                .FirstOrDefaultAsync(d => d.AppUserId == userId);

            if (donor == null)
                throw new Exception("Donor not found");

            // 2) Query campaigns that have at least one DonationOrder from this donor
            var campaigns = await _unitOfWork.Repository<Campaign, int>()
                .FindAllAsync(
                    c => c.DonationOrders.Any(d => d.DonorId == donor.Id),
             include: q => q
                 .Include(c => c.Category)
                 .Include(c => c.DonationOrders)
         );

            // 3) Map to CampaignDto and compute a few helpful aggregates
            var result = campaigns.Select(c =>
            {
                var totalCash = c.DonationOrders.Sum(d => d.CashAmount ?? 0m);
                var donorCount = c.DonationOrders
                    .Where(d => d.DonorId.HasValue)
                    .Select(d => d.DonorId)
                    .Distinct()
                    .Count();

                int progressPercentage = 0;
                if (c.GoalCash.HasValue && c.GoalCash > 0)
                {
                    progressPercentage = (int)Math.Min(100,
                        Math.Round((double)(totalCash / c.GoalCash.Value) * 100));
                }

                return new CampaignDto
                {
                    Id = c.Id,
                    Title = c.Title,
                    ShortDescription = string.IsNullOrEmpty(c.Description)
                        ? string.Empty
                        : (c.Description.Length > 150 ? c.Description.Substring(0, 150) : c.Description),
                    PictureUrl = c.PictureUrl,
                    CategoryName = c.Category?.Name ?? string.Empty,
                    Status = (c.EndDate.HasValue && c.EndDate.Value < DateTime.UtcNow) ? "Completed" : "Active",
                    ProgressPercentage = progressPercentage,
                    DonorCount = donorCount,
                  //  CreateAt = c.CreatedAt ?? DateTime.UtcNow
                };
            }).ToList();

            return result;
        }

        // ---------------------------------------------------------------------------
        // 4) Get Profile To Donor 
        // ---------------------------------------------------------------------------
        public async Task<GetProfileDto> GetProfileAsync(string userId)
        {
            // 1) استخدم UserManager للحصول على AppUser
            var appUser = await _userManager.FindByIdAsync(userId);

            if (appUser == null)
                throw new Exception("User not found");

            // 2) ثم احصل على Donor
            var donor = await _unitOfWork.Repository<Donor, int>()
                .FirstOrDefaultAsync(d => d.AppUserId == userId);

            if (donor is null)
                throw new Exception("Donor not found");

            // 3) Map to DTO
            var profileDto = new GetProfileDto
            {

                FullName = donor.FullName ?? string.Empty,
                NationalId = donor.NationalId ?? string.Empty,
                PhoneNumber = appUser.PhoneNumber ?? string.Empty,
                Address = donor.Address ?? string.Empty,
                Email = appUser.Email ?? string.Empty
            };
            return profileDto;
        }
    }
}

