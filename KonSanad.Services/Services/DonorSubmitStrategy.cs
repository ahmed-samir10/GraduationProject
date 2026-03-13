using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KonSanad.Core.Entity.Enums;
using KonSanad.Repository.Entities;

namespace KonSanad.Services.Services
{
    public class DonorSubmitStrategy : ICartSubmitStrategy
    {
        private readonly KonSanadDbContext _context;

        public DonorSubmitStrategy(KonSanadDbContext context)
        {
            _context = context;
        }

        public async Task<SubmitResultDto> SubmitAsync(int userId, CartResponseDto cart)
        {
            // نوع أول عنصر في الكارت
            var type = cart.Items.FirstOrDefault()?.Type;

            // إنشاء الـ DonationOrder
            var donation = new DonationOrder
            {
                DonorId = userId,
                DonationCategoryStatus = type,
                CreatedAt = DateTime.Now,
                ManagerId = 1,
                OrderDate = DateTime.Now,
                OrderItems = new List<OrderItem>()
            };
            _context.DonationOrders.Add(donation);
            await _context.SaveChangesAsync();

            // معالجة كل العناصر
            foreach (var item in cart.Items)
            {
                switch (item.Type)
                {
                    case "donationItem":
                       
                        var orderItem = new OrderItem
                        {
                            DonationOrderId = donation.Id,
                            SupplyId = item.ProductId ?? 0,
                            Quantity = (int)item.Quantity,
                            ImageUrl = item.imagUrl,
                            CustomSupplyName = item.ProductName,
                            Category = item.Category,
                            Unit = "pce",
                            orderItemStatus = RequestedStatus.Pending,
                            Status = item.ExtraData?["Status"],
                            Description = item.ExtraData?["Description"]
                        };
                        _context.OrderItems.Add(orderItem);
                        await _context.SaveChangesAsync();
                        break;

                    case "donationCash":
                        donation.CashDonation = new CashDonation
                        {
                            Amount = decimal.Parse(item.ExtraData["CashAmount"]),
                            Status = "pending",
                            CreatedAt = DateTime.Now
                        };
                        
                        var CashDonation = new CashDonation
                        {
                            DonationOrderId = donation.Id,
                            Amount = decimal.Parse(item.ExtraData["CashAmount"]),
                            Status = "pending",
                            CampaignId = item.ExtraData.ContainsKey("CampaignId") ? int.Parse(item.ExtraData["CampaignId"]) : (int?)null,
                            HelpOrderId = item.ExtraData.ContainsKey("HelpOrderId") ? int.Parse(item.ExtraData["HelpOrderId"]) : (int?)null,
                            CreatedAt = DateTime.Now,
                            cashDonationStatus = RequestedStatus.Pending
                        };
                        _context.CashDonations.Add(CashDonation);
                        await _context.SaveChangesAsync();
                        var Receipt = new Receipt
                        {
                            DonationOrderId = donation.Id,
                            CashDonationId = CashDonation.Id,
                            DonorId = userId,
                             ManagerId = 1,
                            CreatedAt = DateTime.Now,
                           
                        };
                        _context.Receipts.Add(Receipt);
                        await _context.SaveChangesAsync();
                        CashDonation.receiptId = Receipt.Id;
                         _context.CashDonations.Update(CashDonation);
                        await _context.SaveChangesAsync();
                        break;


                    case "custom":
                      
                        var customOrderItem = new OrderItem
                        {
                            DonationOrderId = donation.Id,
                            CustomSupplyName = item.ProductName,
                            Quantity = (int)item.Quantity,
                            Description = item.ExtraData["Description"],
                            Category = item.Category,
                            Unit = item.ExtraData["Unit"],
                            ImageUrl = item.imagUrl,
                            orderItemStatus = RequestedStatus.Pending,
                            // Status = item.ExtraData?["Status"]
                        };
                        _context.OrderItems.Add(customOrderItem);
                        await _context.SaveChangesAsync();
                        break;
                }
            }

          

            return new SubmitResultDto { Success = true };
        }
    }
}