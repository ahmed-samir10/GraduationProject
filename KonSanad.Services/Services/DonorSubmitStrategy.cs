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
        private readonly INotificationService _notificationService;

        public DonorSubmitStrategy(KonSanadDbContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        public async Task<SubmitResultDto> SubmitAsync(int userId, CartResponseDto cart)
        {
            var type = cart.Items.FirstOrDefault()?.Type;

            // دور على CampaignId في أي item في الكارت
            int? campaignId = null;

            foreach (var item in cart.Items)
            {
                if (item.ExtraData != null &&
                    item.ExtraData.TryGetValue("CampaignId", out var val) &&
                    int.TryParse(val, out var cid) && cid > 0)
                {
                    var exists = await _context.Campaigns.AnyAsync(c => c.Id == cid);
                    if (exists)
                    {
                        campaignId = cid;
                        break; // لقيناه، وقف
                    }
                }
            }

            var donation = new DonationOrder
            {
                DonorId = userId,
                DonationCategoryStatus = type,
                CampaignId = campaignId, // ✅ هيتحط صح دلوقتي
                CreatedAt = DateTime.Now,
                ManagerId = 1,
                OrderDate = DateTime.Now,
                OrderItems = new List<OrderItem>()
            };
            _context.DonationOrders.Add(donation);
            await _context.SaveChangesAsync();

/*            // ✅ شيل الـ donation من الـ tracker عشان EF ميتدخلش في الـ relationships
            _context.Entry(donation).State = EntityState.Detached;*/

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

                     

                        // ✅ نفس الفكرة للـ HelpOrderId
                        int? helpOrderId = null;
                        if (item.ExtraData.ContainsKey("HelpOrderId"))
                        {
                            var parsedHelpOrderId = int.Parse(item.ExtraData["HelpOrderId"]);
                            helpOrderId = parsedHelpOrderId == 0 ? null : parsedHelpOrderId;
                        }
                        //
                        var cashDonation = new CashDonation
                        {
                            DonationOrderId = donation.Id,
                            Amount = decimal.Parse(item.ExtraData["CashAmount"]),
                            CampaignId = campaignId,   // ✅ آمن دلوقتي
                            HelpOrderId = helpOrderId, // ✅ مش هيبعت 0
                            CreatedAt = DateTime.Now,
                            cashDonationStatus = RequestedStatus.Approved
                        };
                        _context.CashDonations.Add(cashDonation);
                        await _context.SaveChangesAsync();
                      

                        var receipt = new Receipt
                        {
                            DonationOrderId = donation.Id,
                            CashDonationId = cashDonation.Id,
                            Status = RequestedStatus.Approved,
                            DonorId = userId,
                            ManagerId = 1,
                            CreatedAt = DateTime.Now,
                        };
                        _context.Receipts.Add(receipt);
                        await _context.SaveChangesAsync();

                        cashDonation.receiptId = receipt.Id;
                        _context.CashDonations.Update(cashDonation);
                        await _context.SaveChangesAsync();

                        if (campaignId.HasValue)
                        {
                            var campaign = await _context.Campaigns
                                .AsNoTracking()
                                .FirstOrDefaultAsync(c => c.Id == campaignId.Value);

                            if (campaign?.GoalCash > 0)
                            {
                                var totalAmount = await _context.CashDonations
                                    .Where(c => c.CampaignId == campaignId.Value)
                                    .SumAsync(c => c.Amount);

                                var previousTotal = totalAmount - cashDonation.Amount;
                                if (previousTotal < campaign.GoalCash && totalAmount >= campaign.GoalCash)
                                {
                                    campaign.CampaignStatus = CampaignStatus.Completed;
                                    _context.Campaigns.Update(campaign);
                                    await _context.SaveChangesAsync();

                                    await _notificationService.NotifyManagersAsync(new NotificationMessageDto
                                    {
                                        Type = NotificationType.CampaignGoalReached,
                                        Title = "الحملة وصلت للهدف",
                                        Message = $"الحملة {campaign.Title} وصلت أو تجاوزت المبلغ المطلوب.",
                                        EntityType = nameof(Campaign),
                                        EntityId = campaign.Id
                                    });
                                }
                            }

                            // CampaignResult Logic
                            var campaignResult = await _context.CampaignResults
                                .FirstOrDefaultAsync(cr => cr.CampaignId == campaignId.Value);

                            if (campaignResult == null)
                            {
                                // اعمل record جديد
                                campaignResult = new CampaignResult
                                {
                                    CampaignId = campaignId.Value,
                                    TotalCashCollected = cashDonation.Amount,
                                    DonorCount = 1,
                                    BeneficiaryCount = 0,
                                    VolunteerCount = 0,
                                    CreatedAt = DateTime.UtcNow,
                                    UpdatedAt = DateTime.UtcNow,
                                    Status = RequestedStatus.Pending,
                                    IsDeleted = false
                                };
                                await _context.CampaignResults.AddAsync(campaignResult);
                            }
                            else
                            {
                                // زود على الموجود
                                campaignResult.TotalCashCollected += cashDonation.Amount;
                                campaignResult.DonorCount += 1;
                                campaignResult.UpdatedAt = DateTime.Now;
                                _context.CampaignResults.Update(campaignResult);
                            }

                            await _context.SaveChangesAsync();
                        }
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

            await _notificationService.NotifyManagersAsync(new NotificationMessageDto
            {
                Type = NotificationType.DonationSubmitted,
                Title = "تبرع جديد",
                Message = $"تم استلام تبرع جديد برقم {donation.Id} ويحتاج متابعة المدير.",
                EntityType = nameof(DonationOrder),
                EntityId = donation.Id
            });

            return new SubmitResultDto { Success = true };
        }
    }
}
