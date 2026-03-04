
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KonSanad.Core.Dtos;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using KonSanad.Repository.Entities;
using KonSanad.Core.Entity;

namespace KonSanad.Services.Services
{
    public class CartService : ICartService
    {
        private readonly IDatabase _redis;
        private readonly KonSanadDbContext _context;

        public CartService(IConnectionMultiplexer redis, KonSanadDbContext context)
        {
            _redis = redis.GetDatabase();
            _context = context;
        }

        private string GetCartKey(string userId, string role)
            => $"cart:{role}:{userId}";

        private async Task SaveCart(string userId, string role, CartResponseDto cart)
        {
            var json = JsonSerializer.Serialize(cart);
            await _redis.StringSetAsync(GetCartKey(userId, role), json);
        }

        public async Task<CartResponseDto> GetCartAsync(string userId, string role)
        {
            var data = await _redis.StringGetAsync(GetCartKey(userId, role));

            if (data.IsNullOrEmpty)
                return new CartResponseDto { CartId = userId };

            return JsonSerializer.Deserialize<CartResponseDto>(data);
        }

        public async Task<string> AddToCartAsync(string userId, string role, object dto)
        {
            var cart = await GetCartAsync(userId, role);

            if (role == "beneficiary")
            {
                var data = dto as AddToCartDto;

                cart.Items.Add(new CartItemDto
                {
                    ProductId = data.ProductId,
                    Quantity = data.QuantityRequested,
                    Type = "beneficiary",
                    ExtraData = new Dictionary<string, string>
                {
                    { "Reason", data.Reason }
                }
                });
            }
            else if (role == "donor")
            {
                switch (dto)
                {
                    case CreateDonationItemRequestDto item:
                        cart.Items.Add(new CartItemDto
                        {
                            ProductId = item.SupplyId,
                            Quantity = item.Quantity,
                            Type = "donationItem",
                            ExtraData = new Dictionary<string, string>
                        {
                            { "Description", item.Description ?? "" },
                            { "Status", item.Status ?? "" }
                        }
                        });
                        break;

                    case CreateDonationCashRequestDto cash:
                        cart.Items.Add(new CartItemDto
                        {
                            Quantity = 1,
                            Type = "donationCash",
                            ExtraData = new Dictionary<string, string>
                        {
                            { "CashAmount", cash.CashAmount?.ToString() ?? "0" }
                        }
                        });
                        break;

                    case CreateCustomDonationItemRequestDto custom:
                        cart.Items.Add(new CartItemDto
                        {
                            Quantity = custom.Quantity,
                            Type = "custom",
                            ExtraData = new Dictionary<string, string>
                        {
                            { "CustomItemName", custom.CustomItemName },
                            { "Unit", custom.Unit ?? "" },
                            { "Description", custom.Description ?? "" }
                        }
                        });
                        break;
                }
            }

            await SaveCart(userId, role, cart);
            return "تمت الإضافة";
        }

        public async Task<string> UpdateItemAsync(string userId, string role, int productId, int quantity)
        {
            var cart = await GetCartAsync(userId, role);

            var item = cart.Items.FirstOrDefault(x => x.ProductId == productId);
            if (item == null) return "غير موجود";

            item.Quantity = quantity;

            await SaveCart(userId, role, cart);
            return "تم التحديث";
        }

        public async Task<string> RemoveItemAsync(string userId, string role, int productId)
        {
            var cart = await GetCartAsync(userId, role);

            var item = cart.Items.FirstOrDefault(x => x.ProductId == productId);
            if (item == null) return "غير موجود";

            cart.Items.Remove(item);

            await SaveCart(userId, role, cart);
            return "تم الحذف";
        }

        public async Task ClearCartAsync(string userId, string role)
        {
            await _redis.KeyDeleteAsync(GetCartKey(userId, role));
        }

        public async Task<SubmitResultDto> SubmitCartAsync(string userId, string role)
        {
            var cart = await GetCartAsync(userId, role);

            if (!cart.Items.Any())
                return new SubmitResultDto { Success = false };

            ICartSubmitStrategy strategy =
                role == "beneficiary"
                ? new BeneficiarySubmitStrategy(_context)
                : new DonorSubmitStrategy(_context);

            var result = await strategy.SubmitAsync(userId, cart);

            if (result.Success)
                await ClearCartAsync(userId, role);

            return result;
        }
    }

}
