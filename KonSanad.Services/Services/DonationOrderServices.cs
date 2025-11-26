using KonSanad.Core.Dtos;
using KonSanad.Core.IRepositery;
using KonSanad.Repository.Data.Persistence;
using KonSanad.Repository.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KonSanad.Services.Services
{
    // KonSanad.Services/Services/DonationOrderService.cs
    public class DonationOrderService : IDonationOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        public DonationOrderService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // removed [FromBody] attribute — service methods should not have MVC attributes
        public async Task<DonationResponseDto> CreateDonationAsync(CreateDonationRequestDto request, int donorId)
        {
            // optional: verify donor exists using _unitOfWork if your domain requires it
            // var donor = await _unitOfWork.Repository<Donor, int>().GetByIdAsync(donorId);
            // if (donor == null) throw new UnauthorizedAccessException("Donor not found");

            try
            {
                // 1. create DonationOrder and set donor id and cash amount
                var donationOrder = new DonationOrder
                {
                     DonorId = donorId,
                    CampaignId = request.CampaignId,
                    CashAmount = request.CashAmount,
                    CreatedAt = DateTime.UtcNow,
                    Status = "Pending",
                    IsDeleted = false,
                    DonationCategoryStatus = "Pending2"
                };

                await _unitOfWork.Repository<DonationOrder, int>().AddAsync(donationOrder);
                // Save now to get donationOrder.Id so child entities can refer to it
                await _unitOfWork.Complete();

                var response = new DonationResponseDto
                {
                    DonationOrderId = donationOrder.Id,
                    CashAmount = request.CashAmount,
                    Items = new List<DonationItemDto>()
                };

                // 2. cash donation (if any) - link to saved donationOrder
                if (request.CashAmount > 0)
                {
                    var cashDonation = new CashDonation
                    {
                        DonationOrderId = donationOrder.Id,
                        Amount = request.CashAmount.Value,
                        CreatedAt = DateTime.UtcNow,
                        Status = "Pending"
                    };

                    await _unitOfWork.Repository<CashDonation, int>().AddAsync(cashDonation);
                    // optional: set response.CashAmount already set above
                }

                // 3. in-kind donations
                if (request.Items != null && request.Items.Any())
                {
                    foreach (var item in request.Items)
                    {
                        // determine supplyId only if provided and exists
                        int? supplyId = null;
                        if (item.SupplyId > 0)
                        {
                            var supplyExists = await _unitOfWork.Repository<Supply, int>().GetByIdAsync(item.SupplyId);
                            if (supplyExists != null)
                                supplyId = item.SupplyId;
                            else
                                supplyId = null; // treat as custom supply (no FK)
                        }

                        var orderItem = new OrderItem
                        {
                            DonationOrderId = donationOrder.Id,
                            SupplyId = supplyId, // now nullable
                            CustomSupplyName = supplyId.HasValue ? null : (item.CustomName ?? "تبرع عيني عام"),
                            Quantity = item.Quantity,
                            Unit = item.Unit,
                            CreatedAt = DateTime.UtcNow,
                            Status = "Pending"
                        };

                        // add order item to context
                        await _unitOfWork.Repository<OrderItem, int>().AddAsync(orderItem);
                        response.Items.Add(item);

                        // update supply stock if applicable
                        if (supplyId.HasValue)
                        {
                            var supply = await _unitOfWork.Repository<Supply, int>().GetByIdAsync(supplyId.Value);
                            if (supply != null)
                            {
                                supply.StockQuantity = (supply.StockQuantity ?? 0) + item.Quantity;
                                _unitOfWork.Repository<Supply, int>().Update(supply);
                            }
                        }

                        // create movement; set SupplyId only when available
                        var movement = new SupplyMovement
                        {
                            SupplyId = supplyId,
                            OrderItem = orderItem,
                            MovementType = "IncomingDonation",
                            MovedQuantity = item.Quantity,
                            MovementDate = DateTime.UtcNow,
                            Status = "Pending"
                        };

                        await _unitOfWork.Repository<SupplyMovement, int>().AddAsync(movement);
                    }
                }

                // Save all remaining changes (order items, movements, cash donation updates, supply updates)
                await _unitOfWork.Complete();

                return response;
            }
            catch (Exception)
            {
                // consider logging the exception before rethrowing
                throw;
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
                                    SupplyId = (int)i.SupplyId,
                                    CustomName = i.Supply != null ? i.Supply.Name : null,
                                    Quantity = i.Quantity
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
