using KonSanad.Core.Entity.Enums;

namespace KonSanad.Services.Services
{
    public class BeneficiarySubmitStrategy : ICartSubmitStrategy
    {
        private readonly KonSanadDbContext _context;
        private readonly INotificationService _notificationService;

        public BeneficiarySubmitStrategy(KonSanadDbContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }


        public async Task<SubmitResultDto> SubmitAsync(int userId, CartResponseDto cart)
        {
            // ✅ Create the HelpOrder with the beneficiary's user ID and set its status to Pending
            var helpOrder = new HelpOrder
            {
                BeneficiaryId = userId,
                CreatedAt = DateTime.Now,
                HelpOrderStatus = RequestedStatus.Pending
            };

            // ✅ Add HelpOrderItems for each item in the cart that is of type "beneficiary"
            foreach (var item in cart.Items.Where(x => x.Type == "beneficiary"))
            {
                helpOrder.HelpOrderItems.Add(new HelpOrderItem
                {
                    SupplyId = item.ProductId.Value,
                    QuantityRequested = (int)item.Quantity,
                    reason = item.Reason,
                    ImageUrl = item.imagUrl,
                    Title = item.ProductName
                });
            }

            // ✅ Save the HelpOrder to the database to generate its ID
            _context.HelpOrders.Add(helpOrder);
            await _context.SaveChangesAsync();


            // ✅ Save supporting files from ALL beneficiary items
            foreach (var item in cart.Items.Where(x => x.Type == "beneficiary"))
            {
                if (item.SupportingFileUrls != null && item.SupportingFileUrls.Any())
                {
                    foreach (var fileUrl in item.SupportingFileUrls)
                    {
                        _context.Set<HelpOrderFile>().Add(new HelpOrderFile
                        {
                            HelpOrderId = helpOrder.Id,
                            FileUrl = fileUrl,
                            CreatedAt = DateTime.Now
                        });
                    }
                }
            }

            // ✅ Save all changes to the database
            await _context.SaveChangesAsync();

            // notifications...
            await _notificationService.NotifyManagersAsync(new NotificationMessageDto
            {
                Type = NotificationType.HelpRequestSubmitted,
                Title = "طلب احتياج جديد",
                Message = $"تم إنشاء طلب احتياج جديد برقم {helpOrder.Id} من أحد المستفيدين.",
                EntityType = nameof(HelpOrder),
                EntityId = helpOrder.Id
            });

            await _notificationService.NotifyRoleAsync("Donor", new NotificationMessageDto
            {
                Type = NotificationType.NeededItemAdded,
                Title = "احتياج جديد",
                Message = "تمت إضافة احتياج جديد ويمكنك الاطلاع عليه والتبرع له.",
                EntityType = nameof(HelpOrder),
                EntityId = helpOrder.Id
            });

            return new SubmitResultDto { Success = true };
        }
    }
}
