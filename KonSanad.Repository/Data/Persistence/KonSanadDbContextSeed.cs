using KonSanad.Core.Entity.Identity;
using Microsoft.AspNetCore.Identity;
using System.Text.Json;

namespace KonSanad.Repository.Data.Persistence
{
    public class KonSanadDbContextSeed : DbContext
    {
        
        public static async Task SeedAsync(KonSanadDbContext context , UserManager<AppUser> userManager)
        {
           

            if (!context.Beneficiaries.Any())
            {
                var filePath = Path.Combine("Beneficiaries.json");
                if (File.Exists(filePath))
                {
                    var brandsData = await File.ReadAllTextAsync(filePath);
                    var brands = JsonSerializer.Deserialize<List<Beneficiary>>(brandsData);
                    if (brands != null && brands.Any())
                    {
                        await context.Beneficiaries.AddRangeAsync(brands);
                        await context.SaveChangesAsync();
                    }
                }
            }

            if (!context.Categories.Any())
            {
                var filePath = Path.Combine( "Categories.json");
                if (File.Exists(filePath))
                {
                    var brandsData = await File.ReadAllTextAsync(filePath);
                    var brands = JsonSerializer.Deserialize<List<Category>>(brandsData);
                    if (brands != null && brands.Any())
                    {
                        await context.Categories.AddRangeAsync(brands);
                        await context.SaveChangesAsync();
                    }
                }
            }

            // 🔥 Manager + Identity User
           /* if (context.Managers.Count() < 2)
            {
                var user = new AppUser()
                {
                    FirstName = "Ahmed",
                    LastName = "Samir",
                    Email = "Manger564@gmail.com",
                    UserName = "Manger564@gmail.com",
                    PhoneNumber = "01015317037",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(user, "Pa$$w0rd");

                if (!result.Succeeded)
                {
                    var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                    throw new InvalidOperationException($"Seeding user failed: {errors}");
                }

                // 👇 ضيف Manager مربوط بالـ User
                var manager = new Manager
                {
                    FullName = user.FirstName + " " + user.LastName,
                    Email = user.Email,
                    Phone = user.PhoneNumber,
                    Address = "Cairo, Egypt",  // ✅ أضف السطر ده
                    Password = "Pa$$w0rd",     // ✅ أضف ده كمان لو الـ column NOT NULL
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow, // ✅ لو موجودة
                    Status = Core.Entity.Enums.RequestedStatus.Pending,
                    IsDeleted = false
                };

                await context.Managers.AddAsync(manager);
                await context.SaveChangesAsync();
            }*/
                if (!context.Supplies.Any())
            {
                var filePath = Path.Combine( "Supplies.json");
                if (File.Exists(filePath))
                {
                    var brandsData = await File.ReadAllTextAsync(filePath);
                    var brands = JsonSerializer.Deserialize<List<Supply>>(brandsData);
                    if (brands != null && brands.Any())
                    {
                        await context.Supplies.AddRangeAsync(brands);
                        await context.SaveChangesAsync();
                    }
                }
            }

            if (!context.Donors.Any())
            {
                var filePath = Path.Combine( "Donors.json");
                if (File.Exists(filePath))
                {
                    var brandsData = await File.ReadAllTextAsync(filePath);
                    var brands = JsonSerializer.Deserialize<List<Donor>>(brandsData);
                    if (brands != null && brands.Any())
                    {
                        await context.Donors.AddRangeAsync(brands);
                        await context.SaveChangesAsync();
                    }
                }
            }

            if (!context.DonationOrders.Any())
            {
                var filePath = Path.Combine( "DonationOrders.json");
                if (File.Exists(filePath))
                {
                    var brandsData = await File.ReadAllTextAsync(filePath);
                    var brands = JsonSerializer.Deserialize<List<DonationOrder>>(brandsData);
                    if (brands != null && brands.Any())
                    {
                        await context.DonationOrders.AddRangeAsync(brands);
                        await context.SaveChangesAsync();
                    }
                }
            }

            if (!context.OrderItems.Any())
            {
                var filePath = Path.Combine( "OrderItems_Seed.json");
                if (File.Exists(filePath))
                {
                    var brandsData = await File.ReadAllTextAsync(filePath);
                    var brands = JsonSerializer.Deserialize<List<OrderItem>>(brandsData);
                    if (brands != null && brands.Any())
                    {
                        await context.OrderItems.AddRangeAsync(brands);
                        await context.SaveChangesAsync();
                    }
                }
            }
        }
    }
}