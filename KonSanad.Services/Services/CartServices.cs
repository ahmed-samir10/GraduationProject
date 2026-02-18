
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KonSanad.Services.Services
{
    public class CartServices : ICartService
    {
        private readonly IDatabase _database;
        private readonly IMapper _mapper;
        private readonly KonSanadDbContext _dbContext;

        public CartServices(IConnectionMultiplexer redis, IMapper mapper , KonSanadDbContext dbContext)
        {
            _database = redis.GetDatabase();
            _mapper = mapper;
            _dbContext = dbContext;
        }

        private string GetCartKey(int userId)
            => $"cart:{userId}";

        // Get Cart
        public async Task<CartResponseDto> GetCartAsync(int userId)
        {
            var data = await _database.StringGetAsync(GetCartKey(userId));

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            CartResponseDto cart;

            if (data.IsNullOrEmpty)
            {
                cart = new CartResponseDto
                {
                    CartId = userId,
                    Items = new List<CartItemResponseDto>()
                };
            }
            else
            {
                cart = JsonSerializer.Deserialize<CartResponseDto>(data, options);

                if (cart?.Items == null)
                    cart.Items = new List<CartItemResponseDto>();
            }

            // تحقق من أي item ناقص تفاصيل وملأها من قاعدة البيانات
            foreach (var item in cart.Items)
            {
                if (string.IsNullOrEmpty(item.ProductName) || string.IsNullOrEmpty(item.ImagUrl))
                {
                    var orderItem = await _dbContext.OrderItems
                        .Include(o => o.Supply)
                        .FirstOrDefaultAsync(o => o.Id == item.ProductId);

                    if (orderItem != null)
                    {
                        item.ProductName = orderItem.Supply?.Name ?? orderItem.CustomSupplyName ?? "Unknown";
                        item.ImagUrl = orderItem.ImageUrl ?? orderItem.Supply?.ImageUrl;
                    }
                }
            }

            return cart;
        }



        // Add Item
        public async Task AddItemAsync(int userId, AddToCartDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var cart = await GetCartAsync(userId);

            // نجلب الـ OrderItem من قاعدة البيانات حسب الـ Id اللي جاي من DTO
            var orderItem = await _dbContext.OrderItems
                .Include(o => o.Supply)
                .FirstOrDefaultAsync(o => o.Id == dto.ProductId);

            if (orderItem == null)
                throw new Exception($"Item with ProductId {dto.ProductId} not found.");

            var item = cart.Items.FirstOrDefault(x => x.ProductId == orderItem.Id);

            if (item == null)
            {
                cart.Items.Add(new CartItemResponseDto
                {
                    ProductId = orderItem.Id,
                    ProductName = orderItem.Supply?.Name ?? orderItem.CustomSupplyName ?? "Unknown",
                    ImagUrl = orderItem.ImageUrl ?? orderItem.Supply?.ImageUrl,
                    Quantity = dto.QuantityRequested,
                });
            }
            else
            {
                item.Quantity += dto.QuantityRequested;
            }

            // Serialize بطريقة مرتبة
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };

            var json = JsonSerializer.Serialize(cart, options);
            await _database.StringSetAsync(GetCartKey(userId), json);
        }



        // Remove Item
        public async Task<string> RemoveItemAsync(int userId, int productId)
        {
            var cart = await GetCartAsync(userId);

            cart.Items.RemoveAll(x => x.ProductId == productId);

            var json = JsonSerializer.Serialize(cart);
            await _database.StringSetAsync(GetCartKey(userId), json);
            return "تم الحذف";
        }

        // Clear Cart
        public async Task<string> ClearCartAsync(int userId)
        {
            await _database.KeyDeleteAsync(GetCartKey(userId));
            return "تم الحذف";
        }

        // Optional: Update Item Quantity
        public async Task<string> UpdateItemQuantityAsync(int userId, int productId, int quantity)
        {
            if (quantity <= 0)
            {
                await RemoveItemAsync(userId, productId);
                return "تم التعديل";
            }

            var cart = await GetCartAsync(userId);
            var item = cart.Items.FirstOrDefault(x => x.ProductId == productId);

            if (item != null)
            {
                item.Quantity = quantity;

                var json = JsonSerializer.Serialize(cart);
                await _database.StringSetAsync(GetCartKey(userId), json);
            }
            return "تم التعديل";
        }
    }

}
