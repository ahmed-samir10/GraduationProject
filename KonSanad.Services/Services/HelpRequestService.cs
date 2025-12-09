using AutoMapper;
using KonSanad.Core.Dtos;
using KonSanad.Core.Entity;
using KonSanad.Core.Entity.Identity;
using KonSanad.Core.IRepositery;
using KonSanad.Repository.Entities;
using KonSanad.Services.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace KonSanad.Services.Services
{
    public class HelpRequestService : IHelpRequestService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly UserManager<AppUser> _userManager;

        public HelpRequestService(IUnitOfWork unitOfWork , IMapper  _mapper, UserManager<AppUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;

        }

        public async Task<HelpRequestResponseDto> RequestExistingProductAsync(int id, ExistingProductRequestDto request)
        {
           
            if (request == null) throw new System.ArgumentNullException(nameof(request));

            // find the order item
            var orderItem = await _unitOfWork.Repository<OrderItem, int>().GetByIdAsync(request.OrderItemId);
            if (orderItem == null) return new HelpRequestResponseDto { HelpOrderId = 0, Message = "Order item not found." };

            // verify this order item belongs to a donation (donor) and is not already assigned to a beneficiary
            var donationOrder = await _unitOfWork.Repository<DonationOrder, int>().GetByIdAsync(orderItem.DonationOrderId);
            if (donationOrder == null) return new HelpRequestResponseDto { HelpOrderId = 0, Message = "Order item does not belong to a donation order." };

            if (!donationOrder.DonorId.HasValue)
                return new HelpRequestResponseDto { HelpOrderId = 0, Message = "This donation has no donor and cannot be requested." };

            // ensure donation is not already assigned to any beneficiary (via Receipts)
            var receipts = await _unitOfWork.Repository<Receipt, int>().GetAllAsync();
            var assigned = receipts.Any(r => r.DonationOrderId == donationOrder.Id && r.BeneficiaryId.HasValue);
            if (assigned)
                return new HelpRequestResponseDto { HelpOrderId = 0, Message = "This donated order is already assigned to a beneficiary." };

            // optional: prevent requesting items that belong to campaigns (uncomment if desired)
            // if (donationOrder.CampaignId.HasValue)
            //     return new HelpRequestResponseDto { HelpOrderId = 0, Message = "This donation belongs to a campaign and cannot be requested here." };

            // create HelpOrder with reason and reference to order item
            var helpOrder = new HelpOrder
            {
                BeneficiaryId = id,
                Description = (string.IsNullOrWhiteSpace(orderItem.CustomSupplyName) ? string.Empty : $" {orderItem.CustomSupplyName}"),
                Status = "قيد الانتظار ",
                IsDeleted = false
            };

            await _unitOfWork.Repository<HelpOrder, int>().AddAsync(helpOrder);
            await _unitOfWork.Complete();

            // ensure we have a Supply record to reference in HelpOrderItem
            int supplyId;
            if (orderItem.SupplyId.HasValue)
            {
                supplyId = orderItem.SupplyId.Value;
            }
            else
            {
                // create a catalog-style Supply for this donated custom item so HelpOrderItem can reference it
                var supply = new Supply
                {
                    Name = string.IsNullOrWhiteSpace(orderItem.CustomSupplyName) ? "Donated Custom Item" : orderItem.CustomSupplyName,
                    Description = $"Donated item from DonationOrder {donationOrder.Id}",
                    Unit = string.IsNullOrWhiteSpace(orderItem.Unit) ? "pcs" : orderItem.Unit,
                    StockQuantity = 0
                };

                await _unitOfWork.Repository<Supply, int>().AddAsync(supply);
                await _unitOfWork.Complete();

                supplyId = supply.Id;
            }

            // create HelpOrderItem using quantity from the donated order item
            var helpItem = new HelpOrderItem
            {
                HelpOrderId = helpOrder.Id,
                SupplyId = supplyId,
                QuantityRequested = orderItem.Quantity,
                ImageUrl = null
            };

            await _unitOfWork.Repository<HelpOrderItem, int>().AddAsync(helpItem);
            await _unitOfWork.Complete();

            return new HelpRequestResponseDto
            {
                HelpOrderId = helpOrder.Id,
                Message = "Request submitted successfully."
            };
        }

        public async Task<HelpRequestResponseDto> RequestNewProductAsync(int id, NewProductRequestDto request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var imageUrl = await SaveImageAsync(request.ImageFile);

            var helpOrder = new HelpOrder
            {
                BeneficiaryId = id,
                Title = request.ProductName,
                Description = request.ProductDescription,
                Status = "Requested",
                IsDeleted = false
            };

            await _unitOfWork.Repository<HelpOrder, int>().AddAsync(helpOrder);
            await _unitOfWork.Complete();

            var supply = new Supply
            {
                Name = string.IsNullOrWhiteSpace(request.ProductName) ? "Custom Requested Product" : request.ProductName,
                Description = string.IsNullOrWhiteSpace(request.Reason) ? "Requested via HelpOrder (new product)" : request.Reason,
                Unit = "pcs",
                StockQuantity = 0
            };

            await _unitOfWork.Repository<Supply, int>().AddAsync(supply);
            await _unitOfWork.Complete();

            var helpItem = new HelpOrderItem
            {
                HelpOrderId = helpOrder.Id,
                SupplyId = supply.Id,
                QuantityRequested = 1,
                ImageUrl = imageUrl  // ← الصورة اللي اتخزنت
            };

            await _unitOfWork.Repository<HelpOrderItem, int>().AddAsync(helpItem);
            await _unitOfWork.Complete();

            return new HelpRequestResponseDto
            {
                HelpOrderId = helpOrder.Id,
                Message = "New product request submitted successfully."
            };
        }



        public async Task<string> JoinCampaignAsync(int userid, JoinCampaignRequestDto request)
        {
            // تحقق لو المستفيد موجود
            var beneficiary = await _unitOfWork.Repository<Beneficiary, int>().GetByIdAsync(userid);
            if (beneficiary == null)
                throw new Exception("Beneficiary not found");

            // تحقق لو الحملة موجودة
            var campaign = await _unitOfWork.Repository<Campaign, int>().GetByIdAsync(request.CampaignId);
            if (campaign == null)
                throw new Exception("Campaign not found");

            // تحقق لو المستفيد قدم طلب قبل كده لنفس الحملة
            var existingRequest = await _unitOfWork.Repository<BeneficiaryCampaign, int>()
                .FirstOrDefaultAsync(bc => bc.BeneficiaryId == userid
                                           && bc.CampaignId == request.CampaignId);
            if (existingRequest != null)
                throw new Exception("Already requested for this campaign");

            var joinRequest = new BeneficiaryCampaign
            {
                BeneficiaryId = userid,
                CampaignId = request.CampaignId,
                RequestedAt = DateTime.UtcNow,
              //  Reason = request.Reason
            };

            // joinRequest.Status = "Pending";


            await _unitOfWork.Repository<BeneficiaryCampaign, int>().AddAsync(joinRequest);

            // احفظ التغييرات
            await _unitOfWork.Complete();

            return "Request submitted successfully";
        }
        public async Task<IEnumerable<CampaignDto>> GetJoinedCampaignsAsync(string userId)
        {
            // 1) نجيب المستفيد حسب AppUserId
            var beneficiary = await _unitOfWork.Repository<Beneficiary, int>()
                .FirstOrDefaultAsync(b => b.AppUserId == userId);

            if (beneficiary == null)
                throw new Exception("Beneficiary not found");

            // 2) نجيب العلاقات بين المستفيد والحملات مع Include
            var joinedCampaigns = await _unitOfWork.Repository<BeneficiaryCampaign, int>()
    .FindAllAsync(
        bc => bc.BeneficiaryId == beneficiary.Id,
        include: q => q
            .Include(x => x.Campaign)
                .ThenInclude(c => c.DonationOrders)  // 👈 أضف هذا
            .Include(x => x.Campaign)
                .ThenInclude(c => c.Category)        // 👈 وهذا للـ CategoryName
    );

            // 3) نتأكد إن Campaign موجود
            var campaignEntities = joinedCampaigns
                .Where(j => j.Campaign != null)
                .Select(j => j.Campaign)
                .ToList();

            // 4) Map
            var campaignDtos = campaignEntities.Select(c => new CampaignDto
            {
                Id = c.Id,
                Title = c.Title ?? string.Empty,
                PictureUrl = c.PictureUrl ?? string.Empty,
                ShortDescription = string.IsNullOrWhiteSpace(c.Description) ? string.Empty :
         c.Description.Length > 180 ? c.Description.Substring(0, 180).Trim() + "…" : c.Description,
                CategoryName = c.Category?.Name ?? string.Empty,
                ProgressPercentage = CampaignMetrics.CalculateProgress(c),
                Status = CampaignMetrics.CalculateStatus(c),
                DonorCount = CampaignMetrics.CalculateDonorCount(c),
              CreateAt = c.CreatedAt
            }).ToList();
            return campaignDtos;
        }

        public async Task<IEnumerable<HelpOrderDto>> GetMyHelpRequestsAsync(string userId)
        {
            // 1) هات المستفيد
            var beneficiary = await _unitOfWork.Repository<Beneficiary, int>()
                .FirstOrDefaultAsync(b => b.AppUserId == userId);

            if (beneficiary == null)
                throw new Exception("Beneficiary not found");

            // 2) هات الطلبات مع Items و Supply
            var requests = await _unitOfWork.Repository<HelpOrder, int>()
                .FindAllAsync(
                    h => h.BeneficiaryId == beneficiary.Id,
                    include: q => q
                        .Include(h => h.HelpOrderItems)
                            .ThenInclude(item => item.Supply) // 👈 مهم لجلب اسم الـ Supply
                );

            // 3) Map إلى DTO
            var helpOrderDtos = requests.Select(h => new HelpOrderDto
            {
                Id = h.Id,
              
                Title = h.Title,
                Description = h.Description ?? string.Empty,
                BeneficiaryId = h.BeneficiaryId,
              
                Status = "قيد الانتظار",
                CreatedAt = h.CreatedAt,
                

             


            }).ToList();

            return helpOrderDtos;
        }




        public async Task<GetProfileDto> GetProfileAsync(string userId)
        {
            // 1) استخدم UserManager للحصول على AppUser
            var appUser = await _userManager.FindByIdAsync(userId);

            if (appUser == null)
                throw new Exception("User not found");

            // 2) ثم احصل على Beneficiary
            var beneficiary = await _unitOfWork.Repository<Beneficiary, int>()
                .FirstOrDefaultAsync(b => b.AppUserId == userId);

            if (beneficiary == null)
                throw new Exception("Beneficiary not found");

            // 3) Map to DTO
            var profileDto = new GetProfileDto
            {
                
                FullName = beneficiary.FullName ?? string.Empty,
                Email = appUser.Email ?? string.Empty,
                PhoneNumber = appUser.PhoneNumber ?? string.Empty,
                Address = beneficiary.Address ?? string.Empty,
                NationalId = beneficiary.NationalId ?? string.Empty
               
            };

            return profileDto;
        }


        public async Task<bool> UpdateProfileAsync(string userId, UpdateProfileDto dto)
        {
            var beneficiaryRepo = _unitOfWork.Repository<Beneficiary, int>();

            var beneficiary = await beneficiaryRepo.FirstOrDefaultAsync(
                b => b.AppUserId == userId
            );

            if (beneficiary == null)
                throw new Exception("Beneficiary not found");

            // Update fields
            beneficiary.FullName = dto.FullName ?? beneficiary.FullName;
            beneficiary.Email = dto.Email ?? beneficiary.Email;

            beneficiary.NationalId = dto.NationalId ?? beneficiary.NationalId;
            beneficiary.Phone = dto.PhoneNumber ?? beneficiary.Phone;
            beneficiary.Address = dto.Address ?? beneficiary.Address;

            _unitOfWork.Repository<Beneficiary, int>().Update(beneficiary);
            await _unitOfWork.Complete();

            return true;
        }


        private async Task<string> SaveImageAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return null;

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/uploads/{fileName}";
        }

        
    }
}