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

            // if the donated item maps to a supply catalog entry, create HelpOrderItem to indicate requested quantity
            if (orderItem.SupplyId.HasValue)
            {
                var helpItem = new HelpOrderItem
                {
                    HelpOrderId = helpOrder.Id,
                    SupplyId = orderItem.SupplyId.Value,
                    QuantityRequested = orderItem.Quantity
                };

                await _unitOfWork.Repository<HelpOrderItem, int>().AddAsync(helpItem);
                await _unitOfWork.Complete();
            }

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

            // store new product request details in HelpOrder.Description as JSON (avoids schema changes)
            var payload = new
            {
                ProductName = request.ProductName,
                Condition = request.Condition,
                Reason = request.Reason,
                ImageUrl = request.ImageUrl
            };
            var json = JsonSerializer.Serialize(payload);

            var helpOrder = new HelpOrder
            {
                BeneficiaryId = beneficiaryId,
                Description = $"NewProductRequest:{json}",
                Status = "Requested",
                IsDeleted = false
            };

            await _unitOfWork.Repository<HelpOrder, int>().AddAsync(helpOrder);
            await _unitOfWork.Complete();

            return new HelpRequestResponseDto
            {
                HelpOrderId = helpOrder.Id,
                Message = "New product request submitted successfully."
            };
        }
    }
}
