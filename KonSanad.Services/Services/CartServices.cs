
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



        // =========================
        // Get Cart
        // =========================
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
                        item.Catgorey = orderItem.Category;
                    }
                }
            }

            return cart;
        }




        // =========================
        // Add Item
        // =========================
        public async Task<string> AddItemAsync(int userId, AddToCartDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var cart = await GetCartAsync(userId);

            // نجلب الـ OrderItem من قاعدة البيانات حسب الـ Id اللي جاي من DTO
            var orderItem = await _dbContext.OrderItems
                .Include(o => o.Supply)
                .FirstOrDefaultAsync(o => o.Id == dto.ProductId);

            if (orderItem == null)
                return " طلب غير موجود";
            if (dto.QuantityRequested > orderItem.Quantity)
                return "  كميه غير صحيحه";


            var item = cart.Items.FirstOrDefault(x => x.ProductId == orderItem.Id);
             
            if (item == null)
            {
                cart.Items.Add(new CartItemResponseDto
                {
                    ProductId = orderItem.Id,
                    Reason = dto.Reason,
                    ProductName = orderItem.Supply?.Name ?? orderItem.CustomSupplyName ?? "Unknown",
                    ImagUrl = orderItem.ImageUrl ?? orderItem.Supply?.ImageUrl,
                    Quantity = dto.QuantityRequested,
                    Catgorey =orderItem.Category
                    
                });
                
            }
            else if (dto.QuantityRequested + item.Quantity <= orderItem.Quantity)
            {
                item.Quantity += dto.QuantityRequested;
            }
            else
            {
                return "كميه غير صحيحه";
            }

                // Serialize بطريقة مرتبة
                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = true
                };

            var json = JsonSerializer.Serialize(cart, options);
            await _database.StringSetAsync(GetCartKey(userId), json);
            return " تم الاضافه";
        }






        // =========================
        // Remove Item
        // =========================
        public async Task<string> RemoveItemAsync(int userId, int productId)
        {
            var cart = await GetCartAsync(userId);

            if (cart == null || cart.Items == null || !cart.Items.Any())
                return "الكارت فاضي";

            var item = cart.Items.FirstOrDefault(x => x.ProductId == productId);

            if (item == null)
                return "المنتج مش موجود في الكارت";

            cart.Items.Remove(item);

            var json = JsonSerializer.Serialize(cart);
            await _database.StringSetAsync(GetCartKey(userId), json);

            return "تم الحذف بنجاح";
        }






        // =========================
        // Clear Cart
        // =========================
        public async Task<string> ClearCartAsync(int userId)
        {
            await _database.KeyDeleteAsync(GetCartKey(userId));
            return "تم الحذف";
        }




        // =========================
        // Optional: Update Item Quantity
        // =========================
        public async Task<string> UpdateItemQuantityAsync(int userId, int productId, int quantity)
        {
            if (quantity <= 0)
            {
                await RemoveItemAsync(userId, productId);
                return "تم التعديل";
            }

            var cart = await GetCartAsync(userId);
            var item = cart.Items.FirstOrDefault(x => x.ProductId == productId);

            var orderItem = await _dbContext.OrderItems
                .Include(o => o.Supply)
                .FirstOrDefaultAsync(o => o.Id == productId);
            if (quantity > orderItem.Quantity)
            {
                return  "خطا في الكميه";
            }
            if (item != null)
            {
                item.Quantity = quantity;

                var json = JsonSerializer.Serialize(cart);
                await _database.StringSetAsync(GetCartKey(userId), json);
            }
            return "تم التعديل";
        }


        // =========================
        // Submit Cart: create a HelpOrder from current cart, save to DB and clear cart
        // Returns the created HelpOrderDto
        // =========================
        public async Task<SubmitResultDto> SubmitCartAsync(int userId, IEnumerable<AddToCartDto>? items = null)
        {
            var result = new SubmitResultDto();

            // Validate provided final items before merging
            if (items != null && items.Any())
            {
                var invalidProvided = new List<string>();

                var providedIds = items.Select(i => i.ProductId).ToList();
                var providedOrderItems = await _dbContext.OrderItems
                    .Where(o => providedIds.Contains(o.Id))
                    .ToListAsync();

                foreach (var dto in items)
                {
                    if (dto.QuantityRequested <= 0)
                        invalidProvided.Add($"Product {dto.ProductId} has invalid quantity {dto.QuantityRequested}.");

                    var found = providedOrderItems.Any(o => o.Id == dto.ProductId);
                    if (!found)
                        invalidProvided.Add($"Product {dto.ProductId} not found.");
                }

                if (invalidProvided.Any())
                {
                    result.Success = false;
                    result.Errors.AddRange(invalidProvided);
                    return result;
                }

                // merge now that provided items are valid
               // await AddItemsAsync(userId, items);
            }

            var cart = await GetCartAsync(userId);

            if (cart.Items == null || !cart.Items.Any())
            {
                result.Success = false;
                result.Errors.Add("Cart is empty.");
                return result;
            }

            // Validate cart items
            var errors = new List<string>();
            foreach (var cartItem in cart.Items)
            {
                if (cartItem.Quantity <= 0)
                    errors.Add($"Product {cartItem.ProductId} has invalid quantity {cartItem.Quantity}.");
            }

            var productIds = cart.Items.Select(i => i.ProductId).ToList();
            var orderItems = await _dbContext.OrderItems
                .Include(o => o.Supply)
                .Where(o => productIds.Contains(o.Id))
                .ToListAsync();

            var missing = productIds.Except(orderItems.Select(o => o.Id)).ToList();
            foreach (var id in missing)
                errors.Add($"Product {id} not found.");

            if (errors.Any())
            {
                result.Success = false;
                result.Errors.AddRange(errors);
                return result;
            }

            // Create HelpOrder entity
            var helpOrder = new HelpOrder
            {
                BeneficiaryId = userId,
                Title = "طلب مساعده من سلة المشتريات",
                Description = "تم انشاء الطلب من سلة المشتريات",
                Status = "قيد الانتظار",
            };

            foreach (var cartItem in cart.Items)
            {
                var orderItem = orderItems.FirstOrDefault(o => o.Id == cartItem.ProductId);

                var rawReason = !string.IsNullOrWhiteSpace(cartItem.Reason)
                    ? cartItem.Reason
                    : (!string.IsNullOrWhiteSpace(orderItem?.Description) ? orderItem.Description : null);

                // sanitize accidental literal "string"
                if (!string.IsNullOrWhiteSpace(rawReason) && rawReason.Trim().ToLower() == "string")
                    rawReason = null;

                var finalReason = !string.IsNullOrWhiteSpace(rawReason) ? rawReason : "Not specified";

                var helpOrderItem = new HelpOrderItem
                {
                    QuantityRequested = cartItem.Quantity,
                    // Prefer the actual Supply.Id if the Supply navigation is loaded, otherwise fall back to OrderItem.SupplyId
                    SupplyId = orderItem?.Supply?.Id ?? orderItem?.SupplyId ?? 0,
                    ImageUrl = cartItem.ImagUrl,
                    reason = finalReason
                };

                helpOrder.HelpOrderItems.Add(helpOrderItem);
            }

            // Save help order
            _dbContext.HelpOrders.Add(helpOrder);
            await _dbContext.SaveChangesAsync();

            // Clear cart
            await ClearCartAsync(userId);

            // Map to DTO
            var dtoResult = new HelpOrderDto
            {
                Id = helpOrder.Id,
                BeneficiaryId = helpOrder.BeneficiaryId,
                Title = helpOrder.Title,
                Description = helpOrder.Description,
                CreatedAt = DateTime.UtcNow,
                Status = helpOrder.Status ?? "قيد الانتظار",
                Items = helpOrder.HelpOrderItems.Select(h => new HelpOrderItemDto
                {
                    Id = h.Id,
                    HelpOrderId = h.HelpOrderId,
                    SupplyId = h.SupplyId,
                    Quantity = h.QuantityRequested,
                    ImageUrl = h.ImageUrl
                }).ToList()
            };

            result.Success = true;
            result.HelpOrder = dtoResult;
            return result;
        }




    }

}
