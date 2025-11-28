using KonSanad.Core.Dtos;
using KonSanad.Core.IRepositery;
using KonSanad.Repository.Entities;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace KonSanad.Services.Services
{
    public class HelpRequestService : IHelpRequestService
    {
        private readonly IUnitOfWork _unitOfWork;

        public HelpRequestService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<HelpRequestResponseDto> RequestExistingProductAsync(int beneficiaryId, ExistingProductRequestDto request)
        {
            if (beneficiaryId <= 0) throw new System.ArgumentException("Invalid beneficiary id", nameof(beneficiaryId));
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
                BeneficiaryId = beneficiaryId,
                Description = $"Request for donated item Id={request.OrderItemId}. Reason: {request.Reason}" +
                              (string.IsNullOrWhiteSpace(orderItem.CustomSupplyName) ? string.Empty : $" (Item: {orderItem.CustomSupplyName})"),
                Status = "Requested",
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

        public async Task<HelpRequestResponseDto> RequestNewProductAsync(int beneficiaryId, NewProductRequestDto request)
        {
            if (beneficiaryId <= 0) throw new System.ArgumentException("Invalid beneficiary id", nameof(beneficiaryId));
            if (request == null) throw new System.ArgumentNullException(nameof(request));

            // keep JSON payload in Description for quick reference
            var payload = new
            {
                ProductName = request.ProductName,
                Condition = request.Status,
                Reason = request.Reason,
                ImageUrl = request.ImageUrl
            };
            var json = JsonSerializer.Serialize(payload);

            var helpOrder = new HelpOrder
            {
                BeneficiaryId = beneficiaryId,
                Description = request.Reason,
                Status = "Requested",
                IsDeleted = false
            };

            // persist help order first (follows existing pattern)
            await _unitOfWork.Repository<HelpOrder, int>().AddAsync(helpOrder);
            await _unitOfWork.Complete();

            // Create a Supply entry for the requested custom product so HelpOrderItem can reference it.
            // Supply requires Name, Description and Unit per configuration, provide minimal sensible defaults.
            var supply = new Supply
            {
                Name = string.IsNullOrWhiteSpace(request.ProductName) ? "Custom Requested Product" : request.ProductName,
                Description = string.IsNullOrWhiteSpace(request.Reason) ? "Requested via HelpOrder (new product)" : request.Reason,
                Unit = "pcs",
                StockQuantity = 0
            };

            await _unitOfWork.Repository<Supply, int>().AddAsync(supply);
            await _unitOfWork.Complete();

            // Create HelpOrderItem referencing the newly created Supply
            var helpItem = new HelpOrderItem
            {
                HelpOrderId = helpOrder.Id,
                SupplyId = supply.Id,
                QuantityRequested = 1,
                ImageUrl = request.ImageUrl
            };

            await _unitOfWork.Repository<HelpOrderItem, int>().AddAsync(helpItem);
            await _unitOfWork.Complete();

            return new HelpRequestResponseDto
            {
                HelpOrderId = helpOrder.Id,
                Message = "New product request submitted successfully."
            };
        }
    }
}