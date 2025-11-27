using KonSanad.Core.Dtos;
using KonSanad.Core.IRepositery;
using KonSanad.Repository.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KonSanad.Services.Services
{
    public class DonationService : IDonationService
    {
        private readonly IDonationOrderService _orderService;
        private readonly IUnitOfWork _unitOfWork;

        public DonationService(IDonationOrderService orderService, IUnitOfWork unitOfWork)
        {
            _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        // delegate creation to the existing order service (keeps single place for creation logic)
        public Task<DonationResponseDto> CreateDonationAsync(CreateDonationRequestDto request, int donorId)
            => _orderService.CreateDonationAsync(request, donorId);

        // delegate existing list-by-donor that returns detailed DTOs
      //  public Task<IEnumerable<DonationResponseDto>> GetDonorDonationsAsync(KonSanad.Core.Dtos.DonorRequetDto v)
      //      => _orderService.GetDonorDonationsAsync(v);

        // delegate existing simple donor history projection
        public Task<IEnumerable<object>> GetDonorDonationsAsync(int donorId)
            => _orderService.GetDonorDonationsAsync(donorId);

        // return receipts (and related info) targeted to a beneficiary
        public async Task<IEnumerable<object>> GetBeneficiaryDonationsAsync(int beneficiaryId)
        {
            if (beneficiaryId <= 0) return Enumerable.Empty<object>();

            var receipts = await _unitOfWork.Repository<Receipt, int>().GetAllAsync();
            var donationOrders = await _unitOfWork.Repository<DonationOrder, int>().GetAllAsync();
            var cashDonations = await _unitOfWork.Repository<CashDonation, int>().GetAllAsync();
            var orderItems = await _unitOfWork.Repository<OrderItem, int>().GetAllAsync();
            var campaigns = await _unitOfWork.Repository<Campaign, int>().GetAllAsync();

            var results = receipts
                .Where(r => r.BeneficiaryId == beneficiaryId)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r =>
                {
                    var order = r.DonationOrderId.HasValue ? donationOrders.FirstOrDefault(d => d.Id == r.DonationOrderId.Value) : null;
                    var cash = r.CashDonationId.HasValue
                        ? cashDonations.FirstOrDefault(c => c.Id == r.CashDonationId.Value)
                        : order?.CashDonation;
                    var itemsCount = order != null
                        ? ((order.OrderItems != null && order.OrderItems.Any()) ? order.OrderItems.Sum(i => i.Quantity)
                            : orderItems.Where(i => i.DonationOrderId == order.Id).Sum(i => i.Quantity))
                        : 0;

                    var campaignTitle = order != null && order.CampaignId.HasValue
                        ? campaigns.FirstOrDefault(c => c.Id == order.CampaignId.Value)?.Title
                        : null;

                    return new
                    {
                        ReceiptId = r.Id,
                        DonationOrderId = r.DonationOrderId,
                        CashDonationId = r.CashDonationId,
                        DonorId = r.DonorId,
                        Date = r.CreatedAt,
                        CashAmount = cash != null ? (decimal?)cash.Amount : (order?.CashAmount ?? 0m),
                        ItemsCount = itemsCount,
                        Campaign = campaignTitle,
                        ReceiptStatus = r.Status
                    };
                })
                .ToList();

            return results;
        }

        // return all donation orders with summary info (cash, items count, last receipt)
        public async Task<IEnumerable<object>> GetAllDonationsAsync()
        {
            var donationOrders = await _unitOfWork.Repository<DonationOrder, int>().GetAllAsync();
            var orderItems = await _unitOfWork.Repository<OrderItem, int>().GetAllAsync();
            var cashDonations = await _unitOfWork.Repository<CashDonation, int>().GetAllAsync();
            var receipts = await _unitOfWork.Repository<Receipt, int>().GetAllAsync();
            var donors = await _unitOfWork.Repository<Donor, int>().GetAllAsync();
            var campaigns = await _unitOfWork.Repository<Campaign, int>().GetAllAsync();

            var results = donationOrders
                .Where(d => !d.IsDeleted)
                .OrderByDescending(d => d.CreatedAt)
                .Select(d =>
                {
                    var cash = cashDonations.FirstOrDefault(c => c.DonationOrderId == d.Id) ?? d.CashDonation;
                    var itemsCount = (d.OrderItems != null && d.OrderItems.Any())
                        ? d.OrderItems.Sum(i => i.Quantity)
                        : orderItems.Where(i => i.DonationOrderId == d.Id).Sum(i => i.Quantity);

                    var donor = donors.FirstOrDefault(x => x.Id == d.DonorId);
                    var campaign = campaigns.FirstOrDefault(c => c.Id == d.CampaignId);
                    var lastReceipt = receipts.Where(r => r.DonationOrderId == d.Id)
                                             .OrderByDescending(r => r.CreatedAt)
                                             .FirstOrDefault();

                    return new
                    {
                        DonationOrderId = d.Id,
                        Date = d.CreatedAt,
                        DonorId = d.DonorId,
                        DonorName = donor?.FullName,
                        CashAmount = cash != null ? (decimal?)cash.Amount : d.CashAmount,
                        ItemsCount = itemsCount,
                        CampaignId = d.CampaignId,
                        CampaignTitle = campaign?.Title,
                        LastReceiptId = lastReceipt?.Id,
                        LastReceiptStatus = lastReceipt?.Status
                    };
                })
                .ToList();

            return results;
        }
    }
}
