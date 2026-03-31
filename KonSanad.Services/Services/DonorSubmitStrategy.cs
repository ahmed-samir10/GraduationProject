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
                            StatusPro = item.ExtraData?["Status"],
                            Description = item.ExtraData?["Description"]
                        };
                        _context.OrderItems.Add(orderItem);
                        await _context.SaveChangesAsync();
                        break;
                    case "donationCash":
                        // ← افصل الـ donation من الـ Tracker عشان EF ميحاولش يعمل Navigation Fix
                        _context.Entry(donation).State = EntityState.Detached;

                        var cashDonation = new CashDonation
                        {
                            DonationOrderId = donation.Id, // Id بس، مش Object
                            Amount = decimal.Parse(item.ExtraData["CashAmount"]),
                            CampaignId = item.ExtraData.ContainsKey("CampaignId")
                                ? int.Parse(item.ExtraData["CampaignId"]) : (int?)null,
                            HelpOrderId = item.ExtraData.ContainsKey("HelpOrderId")
                                ? int.Parse(item.ExtraData["HelpOrderId"]) : (int?)null,
                            CreatedAt = DateTime.Now,
                            cashDonationStatus = RequestedStatus.Pending
                        };

                        _context.CashDonations.Add(cashDonation);
                        await _context.SaveChangesAsync();

                        var receipt = new Receipt
                        {
                            DonationOrderId = donation.Id,
                            CashDonationId = cashDonation.Id,
                            Status = RequestedStatus.Pending,
                            DonorId = userId,
                            ManagerId = 1,
                            CreatedAt = DateTime.Now,
                        };

                        _context.Receipts.Add(receipt);
                        await _context.SaveChangesAsync();

                        // حدّث receiptId بدون Update كامل
                        cashDonation.receiptId = receipt.Id;
                        await _context.SaveChangesAsync();

                        // ← رجّع الـ donation للـ Tracker عشان الـ cases التانية تشتغل
                        _context.Entry(donation).State = EntityState.Unchanged;
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
                             StatusPro = item.ExtraData?["Status"]
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