using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                HelpOrderStatus = HelpOrderStatus.Pending
            };

            foreach (var item in cart.Items.Where(x => x.Type == "beneficiary"))
            {
                helpOrder.HelpOrderItems.Add(new HelpOrderItem
                {
                    SupplyId = item.ProductId.Value,
                    QuantityRequested = (int) item.Quantity,
                    reason = item.ExtraData["Reason"],
                    ImageUrl = item.imagUrl,
                    Title =item.ProductName
                    
                });
            }

            _context.HelpOrders.Add(helpOrder);
            await _context.SaveChangesAsync();

            return new SubmitResultDto { Success = true };
        }
    }
}
