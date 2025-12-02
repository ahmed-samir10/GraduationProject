using AutoMapper;
using KonSanad.Core.Dtos;
using KonSanad.Core.Entity;
using KonSanad.Core.IRepositery;
using KonSanad.Repository.Entities;
using Microsoft.AspNetCore.Http;
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

        public HelpRequestService(IUnitOfWork unitOfWork , IMapper  _mapper)
        {
            _unitOfWork = unitOfWork;

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
                Description = $"Request for donated item Id={request.OrderItemId}. Reason: {request.Reason}" +
                              (string.IsNullOrWhiteSpace(orderItem.CustomSupplyName) ? string.Empty : $" (Item: {orderItem.CustomSupplyName})"),
                Status = request.Status,
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
                Description = request.Reason,
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