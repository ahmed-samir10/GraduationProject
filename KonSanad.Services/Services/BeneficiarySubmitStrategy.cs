using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KonSanad.Core.Entity.Enums;

namespace KonSanad.Services.Services
{
    public class BeneficiarySubmitStrategy : ICartSubmitStrategy
    {
        private readonly KonSanadDbContext _context;

        public BeneficiarySubmitStrategy(KonSanadDbContext context)
        {
            _context = context;
        }

        public async Task<SubmitResultDto> SubmitAsync(int userId, CartResponseDto cart)
        {
            var helpOrder = new HelpOrder
            {
                BeneficiaryId = userId,
                CreatedAt = DateTime.Now,
                HelpOrderStatus = RequestedStatus.Pending
            };

            foreach (var item in cart.Items.Where(x => x.Type == "beneficiary"))
            {
                helpOrder.HelpOrderItems.Add(new HelpOrderItem
                {
                    SupplyId = item.ProductId.Value,
                    QuantityRequested = (int) item.Quantity,
                    reason = item.Reason,
                    ImageUrl = item.imagUrl,
                    Title =item.ProductName
                    
                });
            }
            _context.HelpOrders.Add(helpOrder);
            await _context.SaveChangesAsync();
            var beneficiary = await _context.Beneficiaries.FindAsync(userId);
            var Mission = new Mission
            {
                CreatedAt = DateTime.Now,
                HelpOrderId = helpOrder.Id,
                Title = beneficiary.FullName,
                Description =" زيارة ميدانية لمتابعة أحوال الأسر المحتاجة",
                Address = beneficiary.Address,
                PhoneNumber = beneficiary.Phone,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(7),
                MissionStatus = MissionStatus.New,
                Status = RequestedStatus.Pending,
                ManagerId = 1
            };

            _context.Missions.Add(Mission);
            await _context.SaveChangesAsync();
          

            return new SubmitResultDto { Success = true };
        }
    }
}
