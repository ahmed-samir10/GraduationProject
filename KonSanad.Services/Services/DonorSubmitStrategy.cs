using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KonSanad.Core.Entity.Enums;
using KonSanad.Repository.Entities;
using Microsoft.EntityFrameworkCore;

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
            var type = cart.Items.FirstOrDefault()?.Type;

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

            // ✅ شيل الـ donation من الـ tracker عشان EF ميتدخلش في الـ relationships
            _context.Entry(donation).State = EntityState.Detached;

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
                        var existingCashDonation = await _context.CashDonations
                            .FirstOrDefaultAsync(c => c.DonationOrderId == donation.Id);

                        if (existingCashDonation != null) break;

                        // ✅ تحقق إن الـ CampaignId موجود فعلاً في الداتا بيز
                        int? campaignId = null;
                        if (item.ExtraData.ContainsKey("CampaignId"))
                        {
                            var parsedCampaignId = int.Parse(item.ExtraData["CampaignId"]);
                            var campaignExists = await _context.Campaigns
                                .AnyAsync(c => c.Id == parsedCampaignId);

                            // ✅ لو موجود خد الـ Id، لو مش موجود خليه null
                            campaignId = campaignExists ? parsedCampaignId : null;
                        }

                        // ✅ نفس الفكرة للـ HelpOrderId
                        int? helpOrderId = null;
                        if (item.ExtraData.ContainsKey("HelpOrderId"))
                        {
                            var parsedHelpOrderId = int.Parse(item.ExtraData["HelpOrderId"]);
                            helpOrderId = parsedHelpOrderId == 0 ? null : parsedHelpOrderId;
                        }

                        var cashDonation = new CashDonation
                        {
                            DonationOrderId = donation.Id,
                            Amount = decimal.Parse(item.ExtraData["CashAmount"]),
                            CampaignId = campaignId,   // ✅ آمن دلوقتي
                            HelpOrderId = helpOrderId, // ✅ مش هيبعت 0
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

                        cashDonation.receiptId = receipt.Id;
                        _context.CashDonations.Update(cashDonation);
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