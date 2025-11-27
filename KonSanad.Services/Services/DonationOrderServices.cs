using KonSanad.Core.Dtos;
using KonSanad.Core.IRepositery;
using KonSanad.Repository.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace KonSanad.Services.Services
{
    public class DonationOrderService : IDonationOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private const int DefaultManagerId = 1; // change if you want a different default manager

        public DonationOrderService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // compatibility: calls the new overload with no beneficiary
        public Task<DonationResponseDto> CreateDonationAsync(CreateDonationRequestDto request, int donorId)
            => CreateDonationAsync(request, donorId, beneficiaryId: null);

        // explicit campaign endpoint: set campaign and call core
        public Task<DonationResponseDto> CreateDonationToCampaignAsync(CreateDonationRequestDto request, int donorId, int campaignId)
        {
            request.CampaignId = campaignId;
            return CreateDonationAsync(request, donorId, beneficiaryId: null);
        }

        // explicit beneficiary endpoint: pass beneficiaryId to core
        public Task<DonationResponseDto> CreateDonationToBeneficiaryAsync(CreateDonationRequestDto request, int donorId, int beneficiaryId)
            => CreateDonationAsync(request, donorId, beneficiaryId);

        // core implementation — creates DonationOrder, CashDonation, OrderItems, SupplyMovements and Receipts
        public async Task<DonationResponseDto> CreateDonationAsync(CreateDonationRequestDto request, int donorId, int? beneficiaryId)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            try
            {
                var hasCash = request.CashAmount.HasValue && request.CashAmount.Value > 0;
                var hasItems = request.Items != null && request.Items.Any();

                // create DonationOrder (always created to group the operation)
                var donationOrder = new DonationOrder
                {
                    DonorId = donorId > 0 ? donorId : null,
                    CampaignId = request.CampaignId,
                    CashAmount = hasCash ? request.CashAmount : null,
                    OrderDate = DateTime.UtcNow,
                    Status = "Pending",
                    IsDeleted = false,
                    DonationCategoryStatus = "Pending2",
                    ManagerId = null
                };

                await _unitOfWork.Repository<DonationOrder, int>().AddAsync(donationOrder);
                var saved = await _unitOfWork.Complete();
                if (saved <= 0) throw new InvalidOperationException("Failed to create DonationOrder.");

                var response = new DonationResponseDto
                {
                    DonationOrderId = donationOrder.Id,
                    CashAmount = request.CashAmount,
                    Items = new List<DonationItemDto>()
                };

                int? cashReceiptId = null;
                int? itemsReceiptId = null;

                // cash-only
                if (hasCash && !hasItems)
                {
                    var cashDonation = new CashDonation
                    {
                        DonationOrderId = donationOrder.Id,
                        Amount = request.CashAmount.Value
                    };

                    await _unitOfWork.Repository<CashDonation, int>().AddAsync(cashDonation);
                    await _unitOfWork.Complete(); // generate id

                    var cashReceipt = new Receipt
                    {
                        DonationOrderId = donationOrder.Id,
                        CashDonationId = cashDonation.Id,
                        DonorId = donorId > 0 ? donorId : null,
                        ManagerId = DefaultManagerId,
                        BeneficiaryId = beneficiaryId // will be null for campaign flow
                    };

                    await _unitOfWork.Repository<Receipt, int>().AddAsync(cashReceipt);
                    await _unitOfWork.Complete();
                    cashReceiptId = cashReceipt.Id;

                    response.Message = "تم تسجيل تبرع نقدي وتم إصدار إيصال الدفع.";
                    response.TotalItems = 0;
                    response.CashReceiptId = cashReceiptId;
                    return response;
                }

                // items-only
                if (hasItems && !hasCash)
                {
                    foreach (var item in request.Items)
                    {
                        int? supplyId = null;
                        if (item.SupplyId > 0)
                        {
                            var supplyExists = await _unitOfWork.Repository<Supply, int>().GetByIdAsync(item.SupplyId);
                            if (supplyExists != null) supplyId = item.SupplyId;
                        }

                        var orderItem = new OrderItem
                        {
                            DonationOrderId = donationOrder.Id,
                            SupplyId = supplyId,
                            CustomSupplyName = supplyId.HasValue ? null : (item.CustomName ?? "تبرع عيني"),
                            Quantity = item.Quantity,
                            Unit = item.Unit ?? string.Empty,
                            Status = "Pending"
                        };

                        await _unitOfWork.Repository<OrderItem, int>().AddAsync(orderItem);
                        response.Items.Add(item);

                        if (supplyId.HasValue)
                        {
                            var supply = await _unitOfWork.Repository<Supply, int>().GetByIdAsync(supplyId.Value);
                            if (supply != null)
                            {
                                supply.StockQuantity = (supply.StockQuantity ?? 0) + item.Quantity;
                                _unitOfWork.Repository<Supply, int>().Update(supply);
                            }
                        }

                        var movement = new SupplyMovement
                        {
                            SupplyId = supplyId,
                            OrderItem = orderItem,
                            MovementType = "IncomingDonation",
                            MovedQuantity = item.Quantity,
                            MovementDate = DateTime.UtcNow,
                            Status = "Pending",
                            ManagerId = DefaultManagerId
                        };

                        await _unitOfWork.Repository<SupplyMovement, int>().AddAsync(movement);
                    }

                    var affected = await _unitOfWork.Complete();
                    if (affected <= 0) throw new InvalidOperationException("Failed to save donation items.");

                    var itemsReceipt = new Receipt
                    {
                        DonationOrderId = donationOrder.Id,
                        DonorId = donorId > 0 ? donorId : null,
                        ManagerId = DefaultManagerId,
                        BeneficiaryId = beneficiaryId // set when donating to beneficiary
                    };

                    await _unitOfWork.Repository<Receipt, int>().AddAsync(itemsReceipt);
                    await _unitOfWork.Complete();
                    itemsReceiptId = itemsReceipt.Id;

                    response.Message = "تم تسجيل تبرع عيني وتم إصدار إيصال الاستلام.";
                    response.TotalItems = response.Items.Sum(i => i.Quantity);
                    response.ItemsReceiptId = itemsReceiptId;
                    return response;
                }

                // both cash & items => create both and separate receipts
                if (hasCash && hasItems)
                {
                    var cashDonation = new CashDonation
                    {
                        DonationOrderId = donationOrder.Id,
                        Amount = request.CashAmount.Value
                    };

                    await _unitOfWork.Repository<CashDonation, int>().AddAsync(cashDonation);
                    await _unitOfWork.Complete();

                    var cashReceipt = new Receipt
                    {
                        DonationOrderId = donationOrder.Id,
                        CashDonationId = cashDonation.Id,
                        DonorId = donorId > 0 ? donorId : null,
                        ManagerId = DefaultManagerId,
                        BeneficiaryId = beneficiaryId
                    };

                    await _unitOfWork.Repository<Receipt, int>().AddAsync(cashReceipt);
                    await _unitOfWork.Complete();
                    cashReceiptId = cashReceipt.Id;

                    foreach (var item in request.Items)
                    {
                        int? supplyId = null;
                        if (item.SupplyId > 0)
                        {
                            var supplyExists = await _unitOfWork.Repository<Supply, int>().GetByIdAsync(item.SupplyId);
                            if (supplyExists != null) supplyId = item.SupplyId;
                        }

                        var orderItem = new OrderItem
                        {
                            DonationOrderId = donationOrder.Id,
                            SupplyId = supplyId,
                            CustomSupplyName = supplyId.HasValue ? null : (item.CustomName ?? "تبرع عيني"),
                            Quantity = item.Quantity,
                            Unit = item.Unit ?? string.Empty,
                            Status = "Pending"
                        };

                        await _unitOfWork.Repository<OrderItem, int>().AddAsync(orderItem);
                        response.Items.Add(item);

                        if (supplyId.HasValue)
                        {
                            var supply = await _unitOfWork.Repository<Supply, int>().GetByIdAsync(supplyId.Value);
                            if (supply != null)
                            {
                                supply.StockQuantity = (supply.StockQuantity ?? 0) + item.Quantity;
                                _unitOfWork.Repository<Supply, int>().Update(supply);
                            }
                        }

                        var movement = new SupplyMovement
                        {
                            SupplyId = supplyId,
                            OrderItem = orderItem,
                            MovementType = "IncomingDonation",
                            MovedQuantity = item.Quantity,
                            MovementDate = DateTime.UtcNow,
                            Status = "Pending",
                            ManagerId = DefaultManagerId
                        };

                        await _unitOfWork.Repository<SupplyMovement, int>().AddAsync(movement);
                    }

                    var affected = await _unitOfWork.Complete();
                    if (affected <= 0) throw new InvalidOperationException("Failed to save donation items.");

                    var itemsReceipt = new Receipt
                    {
                        DonationOrderId = donationOrder.Id,
                        DonorId = donorId > 0 ? donorId : null,
                        ManagerId = DefaultManagerId,
                        BeneficiaryId = beneficiaryId
                    };

                    await _unitOfWork.Repository<Receipt, int>().AddAsync(itemsReceipt);
                    await _unitOfWork.Complete();
                    itemsReceiptId = itemsReceipt.Id;

                    response.Message = "تم تسجيل تبرع نقدي وعيني. تم إصدار إيصال لكل عملية.";
                    response.TotalItems = response.Items.Sum(i => i.Quantity);
                    response.CashReceiptId = cashReceiptId;
                    response.ItemsReceiptId = itemsReceiptId;
                    return response;
                }

                // fallback
                response.Message = "لم يتم تقديم تبرع صالح.";
                response.TotalItems = 0;
                return response;
            }
            catch (DbUpdateException dbEx)
            {
                throw new InvalidOperationException("Database update failed while creating donation order.", dbEx);
            }
        }

        public async Task<IEnumerable<DonationResponseDto>> GetDonorDonationsAsync(KonSanad.Core.Dtos.DonorRequetDto v)
        {
            if (v == null) throw new ArgumentNullException(nameof(v));

            var allDonations = await _unitOfWork.Repository<DonationOrder, int>().GetAllAsync();

            var donations = allDonations
                .Where(d => d.DonorId == v.DonerId && !d.IsDeleted)
                .OrderByDescending(d => d.CreatedAt)
                .Select(d => new DonationResponseDto
                {
                    DonationOrderId = d.Id,
                    CashAmount = d.CashDonation != null ? d.CashDonation.Amount : 0m,
                    Items = d.OrderItems != null
                                ? d.OrderItems.Select(i => new DonationItemDto
                                {
                                    SupplyId = i.SupplyId ?? 0,
                                    CustomName = i.Supply != null ? i.Supply.Name : i.CustomSupplyName,
                                    Quantity = i.Quantity,
                                    Unit = i.Unit
                                }).ToList()
                                : new List<DonationItemDto>()
                })
                .Take(50)
                .ToList();

            return donations;
        }

        public async Task<IEnumerable<object>> GetDonorDonationsAsync(int donorId)
        {
            var allDonations = await _unitOfWork.Repository<DonationOrder, int>().GetAllAsync();

            var donations = allDonations
                .Where(d => d.DonorId == donorId && !d.IsDeleted)
                .OrderByDescending(d => d.CreatedAt)
                .Select(d => new
                {
                    d.Id,
                    Date = d.CreatedAt.ToString("yyyy-MM-dd"),
                    CashAmount = d.CashDonation != null ? d.CashDonation.Amount : 0m,
                    ItemsCount = d.OrderItems != null ? d.OrderItems.Sum(i => i.Quantity) : 0,
                    Campaign = d.Campaign != null ? d.Campaign.Title : null,
                    Status = d.Status
                })
                .Take(50)
                .ToList();

            return donations;
        }
    }
}
