using AutoMapper;
using KonSanad.Core.Dtos;
using KonSanad.Core.Entity.Identity;
using KonSanad.Core.IRepositery;
using KonSanad.Repository.Entities;
using Microsoft.AspNetCore.Identity;
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

        // ---------------------------------------------------------------------------
        // 3) Create Donation (Cash + Items)
        // ---------------------------------------------------------------------------
        public async Task<DonationResponseItemDto> CreateDonationItemAsync(
          CreateDonationItemRequestDto request, int donorId)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            // Create donation order
            var donationOrder = new DonationOrder
            {
                DonorId = donorId,
                CampaignId = request.CampaignId,
                ManagerId = 1,
                CreatedAt = DateTime.Now,
                OrderDate = DateTime.Now,
                DonationCategoryStatus = "معلقة",
                Status = "قيد المراجعة",
            };

            await _unitOfWork.Repository<DonationOrder, int>().AddAsync(donationOrder);
            await _unitOfWork.Complete();

            // Add existing items
            foreach (var item in request.Items)
            {
                var donationItem = new OrderItem
                {
                    CustomSupplyName = item.CustomName,
                    DonationOrderId = donationOrder.Id,
                    SupplyId = item.SupplyId,
                    Quantity = item.Quantity,
                    ImageUrl = item.ImageFile,
                };

                await _unitOfWork.Repository<OrderItem, int>().AddAsync(donationItem);

                // Update stock
                var supply = await _unitOfWork.Repository<Supply, int>().GetByIdAsync(item.SupplyId);
                if (supply != null)
                {
                    supply.StockQuantity += item.Quantity;
                    _unitOfWork.Repository<Supply, int>().Update(supply);
                }
            }

            await _unitOfWork.Complete();

            return new DonationResponseItemDto
            {
                TotalItems = request.Items.Sum(i => i.Quantity),
                Items = request.Items,
                Message = "تم تسجيل التبرع بالمنتجات الموجودة ✔"
            };
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


        // ---------------------------------------------------------------------------
        // 5) Update Profile To Donor
        // ---------------------------------------------------------------------------
        public async Task<bool> UpdateProfileAsync(string userId, UpdateProfileDto dto)
        {
            var donorRebo = _unitOfWork.Repository<Donor, int>();

            var donor = await donorRebo.FirstOrDefaultAsync(
                d => d.AppUserId == userId
            );

            //var dtos = dto;

            //foreach (var prop in typeof(UpdateProfileDto).GetProperties())
            //{
            //    var value = prop.GetValue(dtos) as string;
            //    if (value == "string")
            //    {
            //        prop.SetValue(dtos, null);
            //    }
            //}

            if (donor is null)
                throw new Exception("Donor not found");

            // Update fields only when a value is provided (non-null/non-whitespace)
            if (!string.IsNullOrWhiteSpace(dto.FullName) && !(dto.FullName == "string"))
                donor.FullName = dto.FullName;

            if (!string.IsNullOrWhiteSpace(dto.NationalId) && !(dto.NationalId == "string"))
                donor.NationalId = dto.NationalId;

            if (!string.IsNullOrWhiteSpace(dto.PhoneNumber) && !(dto.PhoneNumber == "string"))
                donor.Phone = dto.PhoneNumber;

            if (!string.IsNullOrWhiteSpace(dto.Address) && !(dto.Address == "string"))
                donor.Address = dto.Address;

            // If email is provided, update Identity user email (and optionally beneficiary.Email)
            if (!string.IsNullOrWhiteSpace(dto.Email) && !(dto.Email == "string"))
            {
                var appUser = await _userManager.FindByIdAsync(userId);
                if (appUser is null)
                    throw new Exception("App user not found");

                if (!string.Equals(appUser.Email, dto.Email, StringComparison.OrdinalIgnoreCase))
                {
                    appUser.Email = dto.Email;
                    // Do not change UserName unless your system expects it; uncomment if needed:
                    // appUser.UserName = dto.Email;

                    var result = await _userManager.UpdateAsync(appUser);
                    if (!result.Succeeded)
                    {
                        var errors = string.Join(';', result.Errors.Select(e => e.Description));
                        throw new Exception($"Failed to update identity user: {errors}");
                    }
                }

                // Keep beneficiary.Email in sync if you store it there too
                donor.Email = dto.Email;
            }
            // If PhoneNumber is provided, update Identity user phone number
            if (!string.IsNullOrWhiteSpace(dto.PhoneNumber) && !(dto.PhoneNumber == "string"))
            {
                var appUser = await _userManager.FindByIdAsync(userId);
                if (appUser is null)
                    throw new Exception("App user not found");
                if (!string.Equals(appUser.PhoneNumber, dto.PhoneNumber, StringComparison.OrdinalIgnoreCase))
                {
                    var result = await _userManager.SetPhoneNumberAsync(appUser, dto.PhoneNumber);
                    if (!result.Succeeded)
                    {
                        var errors = string.Join(';', result.Errors.Select(e => e.Description));
                        throw new Exception($"Failed to update phone number: {errors}");
                    }
                }

                // Keep donor.Phone in sync
                donor.Phone = dto.PhoneNumber;
            }


            _unitOfWork.Repository<Donor, int>().Update(donor);
            await _unitOfWork.Complete();

            return true;
        }
    }
}

