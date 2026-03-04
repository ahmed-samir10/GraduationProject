using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KonSanad.Services.Services
{
    public class DonorSubmitStrategy : ICartSubmitStrategy
    {
        private readonly KonSanadDbContext _context;
       

        public DonorSubmitStrategy(KonSanadDbContext context )
        {
            _context = context;
          
        }

        public async Task<SubmitResultDto> SubmitAsync(int userId, CartResponseDto cart)
        {
            
            var donation = new DonationOrder
            {
                DonorId = userId,
                CreatedAt = DateTime.Now
            };

            foreach (var item in cart.Items)
            {
                switch (item.Type)
                {
                    case "donationItem":
                        donation.OrderItems.Add(new OrderItem
                        {
                            SupplyId = item.ProductId.Value,
                            Quantity = item.Quantity,
                            Description = item.ExtraData?["Description"]
                        });
                        break;

                    case "donationCash":
                        donation.CashAmount += decimal.Parse(item.ExtraData["CashAmount"]);
                        break;

                    case "custom":
                        donation.OrderItems.Add(new OrderItem
                        {
                            CustomSupplyName = item.ExtraData["CustomItemName"],
                            Quantity = item.Quantity,
                            Description = item.ExtraData["Description"]
                        });
                        break;
                }
            }

            _context.DonationOrders.Add(donation);
            await _context.SaveChangesAsync();

            return new SubmitResultDto { Success = true };
        }
    }
}
