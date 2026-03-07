
using KonSanad.Core.Dtos;
using KonSanad.Core.Entity;
using KonSanad.Core.IRepositery;
using KonSanad.Repository.Entities;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace KonSanad.Services.Services
{
    public class CartService : ICartService
    {
        private readonly IDatabase _redis;
        private readonly KonSanadDbContext _context;
        private readonly IFileService _fileService;

        public CartService(IConnectionMultiplexer redis, KonSanadDbContext context , IFileService fileService)
        {
            _redis = redis.GetDatabase();
            _context = context;
            _fileService = fileService;
        }

        private string GetCartKey(int userId, string role)
            => $"cart:{role}:{userId}";

        private async Task SaveCart(int userId, string role, CartResponseDto cart)
        {
            var json = JsonSerializer.Serialize(cart);
            await _redis.StringSetAsync(GetCartKey(userId, role), json);
        }

        public async Task<CartResponseDto> GetCartAsync(int userId, string role)
        {
            var data = await _redis.StringGetAsync(GetCartKey(userId, role));

            if (data.IsNullOrEmpty)
                return new CartResponseDto { CartId = userId };


            return JsonSerializer.Deserialize<CartResponseDto>(data);
        }

        public async Task<string> AddToCartAsync(int userId, string role, object dto)
        {
            // جلب الكارت أو إنشاء جديد لو مش موجود
            var cart = await GetCartAsync(userId, role) ?? new CartResponseDto
            {
                CartId = userId,
                Items = new List<CartItemDto>()
            };

            if (role == "beneficiary")
            {
                var data = dto as AddToCartDto;
                if (data == null)
                    return "dto must be AddToCartDto for beneficiary";

                // جلب المنتج من الداتا بيز
                var product = await _context.OrderItems
                    .FirstOrDefaultAsync(p => p.Id == data.ProductId);
                if(product == null)
                    return " طلب غير موجود";
                if (data.QuantityRequested > product.Quantity)
                    return "  كميه غير صحيحه";
                  cart.Items = cart.Items ?? new List<CartItemDto>();
                var existingItem = cart.Items.FirstOrDefault(x => x.ProductId == product.Id);

                if (existingItem != null)
                {
                    // المنتج موجود → زود الكمية
                    if (existingItem.Quantity + data.QuantityRequested > product.Quantity)
                        return "الكمية المطلوبة أكبر من المتاح";

                    existingItem.Quantity += data.QuantityRequested;
                }


                else {  cart.Items.Add(new CartItemDto
                {
                    ProductId = product.Id,
                    Quantity = data.QuantityRequested,
                    imagUrl = product.ImageUrl,
                    ProductName = product.CustomSupplyName,
                    Category = product.Category,
                    Type = "beneficiary",
                    Reason = data.Reason
                });}
               
            }
            else if (role == "donor")
            {
               
                switch (dto)
                {
                    case CreateDonationItemRequestDto item:
                        var data = dto as CreateDonationItemRequestDto;
                        var relativeUrl = await _fileService.SaveFileAsync(data.ImageFile, "uploads");
                        var product = await _context.Supplies
                            .FirstOrDefaultAsync(p => p.Id == item.SupplyId);
                        if (product == null)
                            return " طلب غير موجود";

                        cart.Items = cart.Items ?? new List<CartItemDto>();

                        var existingItem = cart.Items
                            .FirstOrDefault(i => i.ProductId == item.SupplyId && i.Type == "donationItem");

                        if (existingItem != null)
                        {
                            existingItem.Quantity += item.Quantity;
                        }
                        else
                        {
                            cart.Items.Add(new CartItemDto
                            {
                                ProductId = item.SupplyId,
                                Quantity = item.Quantity,
                                imagUrl =relativeUrl, // لو فيه صورة من المستخدم استخدمها، لو لا استخدم صورة المنتج
                                ProductName = product.Name,
                                Category = product.Title,
                                Type = "donationItem",
                                ExtraData = new Dictionary<string, string>
        {
            { "Description", item.Description ?? "" },
            { "Status", item.Status ?? "" }
        }
                            });
                        }
                        break;

                    case CreateDonationCashRequestDto cash:

                        cart.Items = cart.Items ?? new List<CartItemDto>();
                        cart.Items.Add(new CartItemDto
                        {
                           
                            Type = "donationCash",
                            ExtraData = new Dictionary<string, string>
                    {

                                    { "CashAmount", cash.CashAmount.ToString() },
                                    { "CampaignId", cash.CampaignId.ToString() },
                                     { "HelpOrderId", cash.HelpOrderId.ToString() }
                    }
                        });
                        break;

                    case CreateCustomDonationItemRequestDto custom:

                        var data2 = dto as CreateCustomDonationItemRequestDto;
                        var relativeUrl2 = await _fileService.SaveFileAsync(data2.ImageFile, "uploads");
                        cart.Items = cart.Items ?? new List<CartItemDto>();
                        cart.Items.Add(new CartItemDto
                        {
                            ProductId =0, // لأن المنتج مخصص مش موجود في الداتا بيز

                            ProductName = custom.CustomItemName,
                            imagUrl = relativeUrl2,
                            Quantity = custom.Quantity,
                            Category = "Custom",
                            Type = "custom",
                            ExtraData = new Dictionary<string, string>
                    {
                        
                        { "Unit", custom.Unit ?? "" },
                        { "Description", custom.Description ?? "" },
                                  { "Status", custom.Status ?? "" }
                    }
                        });
                        break;

                    default:
                        throw new ArgumentException("Invalid DTO type for donor");
                }
            }

            // حفظ الكارت
            await SaveCart(userId, role, cart);

            return "تمت الإضافة";
        }
        public async Task<string> UpdateItemAsync(int userId, string role, int productId, int quantity)
        {
            

            var cart = await GetCartAsync(userId, role);

            var item = cart.Items.FirstOrDefault(x => x.ProductId == productId);
         
            if (item == null) return "غير موجود";
            // لو المستخدم متبرع نتحقق من الكمية المتاحة
            if (role == "Beneficiary")
            {
                var product = await _context.OrderItems
                       .FirstOrDefaultAsync(p => p.Id == productId);
                if (product == null)
                    return "منتج غير موجود";
                if (quantity > product.Quantity)
                    return "خطا في الكميه";
                
            }
            // لو المستخدم متبرع نتحقق من الكمية المتاحة
            if (role == "Donor")
            {
                

                item.Quantity = quantity;

                await SaveCart(userId, role, cart);
                return "تم التحديث";
            }

            item.Quantity = quantity;

            await SaveCart(userId, role, cart);

            return "تم التعديل";
        }


public async Task<string> RemoveItemAsync(int userId, string role, int productId)
        {
            var cart = await GetCartAsync(userId, role);

            var item = cart.Items.FirstOrDefault(x => x.ProductId == productId);
            if (item == null) return "غير موجود";

            cart.Items.Remove(item);

            await SaveCart(userId, role, cart);
            return "تم الحذف";
        }

        public async Task ClearCartAsync(int userId, string role)
        {
            await _redis.KeyDeleteAsync(GetCartKey(userId, role));
        }

        public async Task<SubmitResultDto> SubmitCartAsync(int userId, string role)
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
