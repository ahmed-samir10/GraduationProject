using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using KonSanad.Core.Entities;
using KonSanad.Repository.Entities;
using Microsoft.EntityFrameworkCore;

namespace KonSanad.Repository.Data.Persistence
{
    public class KonSanadDbContextSeed : DbContext
    {
        public static async Task SeedAsync(KonSanadDbContext context)
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

            if (!context.Managers.Any())
            {
                var filePath = Path.Combine( "Managers.json");
                if (File.Exists(filePath))
                {
                    var brandsData = await File.ReadAllTextAsync(filePath);
                    var brands = JsonSerializer.Deserialize<List<Manager>>(brandsData);
                    if (brands != null && brands.Any())
                    {
                        await context.Managers.AddRangeAsync(brands);
                        await context.SaveChangesAsync();
                    }
                }
            }

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