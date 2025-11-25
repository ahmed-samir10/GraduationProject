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
        public async Task<DonationResponseDto> CreateDonationAsync(CreateDonationRequestDto request,[FromBody] int donorId)
        {
            // التحقق من وجود المتبرع
          //  var donor =  await _unitOfWork.Repository<Donor , int>().GetByIdAsync(donorId);
          //  if (donor == null)
          //      throw new UnauthorizedAccessException("المتبرع غير موجود");

         
            try
            {
                // 1. إنشاء DonationOrder
                var donationOrder = new DonationOrder
                {
                    DonorId = donorId,
                    CampaignId = request.CampaignId,
                   
                    CreatedAt = DateTime.UtcNow,
                    Status = "Pending", // أو "Received" لو استلام فوري
                    IsDeleted = false
                };

                await  _unitOfWork.Repository<DonationOrder , int>().AddAsync(donationOrder);
                  await   _unitOfWork.Complete(); // عشان نأخذ الـ Id

                var response = new DonationResponseDto
                {
                    DonationOrderId = donationOrder.Id,
                    CashAmount = request.CashAmount,
                    Items = new List<DonationItemDto>()
                };

                // 2. تبرع نقدي
                if (request.CashAmount > 0)
                {
                    var cashDonation = new CashDonation
                    {
                        DonationOrderId = donationOrder.Id,
                        Amount = request.CashAmount.Value,
                        CreatedAt = DateTime.UtcNow,
                        Status = "Pending"
                    };

                await  _unitOfWork.Repository<CashDonation ,int >().AddAsync(cashDonation);
                    response.CashAmount = request.CashAmount;
                }

                // 3. تبرع عيني
                if (request.Items != null && request.Items.Any())
                {
                    foreach (var item in request.Items)
                    {
                        var orderItem = new OrderItem
                        {
                            
                            DonationOrderId = donationOrder.Id,
                            SupplyId = item.SupplyId > 0 ? item.SupplyId : 0,
                            CustomSupplyName = item.SupplyId > 0 ? null : (item.CustomName ?? "تبرع عيني عام"),
                            Quantity = item.Quantity,
                            Unit = item.Unit,
                            CreatedAt = DateTime.UtcNow,
                            Status = "Pending"
                        };

                      await _unitOfWork.Repository<OrderItem , int>().AddAsync(orderItem);
                        response.Items.Add(item);

                        // تحديث المخزون)
                        if (item.SupplyId > 0)
                        {
                            var supply = await _unitOfWork.Repository<Supply,int>().GetByIdAsync(item.SupplyId);

                            if (supply != null)
                            {
                                supply.StockQuantity  += item.Quantity;
                               _unitOfWork.Repository<Supply,int>().Update(supply);
                            }
                        }

                        // إضافة حركة مخزنية)
                        var movement = new SupplyMovement
                        {
                            SupplyId = item.SupplyId > 0 ? item.SupplyId : null,
                            OrderItemId = orderItem.Id,
                            MovementType = "IncomingDonation",
                            MovedQuantity = item.Quantity,
                            MovementDate = DateTime.UtcNow,
                            Status = "Pending"
                        };
                     await    _unitOfWork.Repository<SupplyMovement ,int>().AddAsync(movement);
                    }
                }

                 _unitOfWork.Complete();
        

                return response;
            }
            catch (Exception)
            {
       
                throw;
            }
        }

        public async Task<IEnumerable<object>> GetDonorDonationsAsync(int donorId)
        {
           
            var allDonations = await _unitOfWork.Repository<DonationOrder ,int>().GetAllAsync();

            
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
