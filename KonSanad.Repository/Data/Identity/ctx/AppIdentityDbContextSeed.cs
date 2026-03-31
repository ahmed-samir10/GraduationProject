using KonSanad.Core.Entity.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace KonSanad.Repository.Data.Identity.ctx
{
    public class AppIdentityDbContextSeed
    {
     
        public static async Task SeedUsersAsync(UserManager<AppUser> userManager)
        {
            // لو مفيش Manager موجود
            if (await userManager.FindByEmailAsync("manager@konsanad.com") == null)
            {
                var manager = new AppUser
                {    FirstName = "Manager",
                PhoneNumber = "01000000000",
                EmailConfirmed = true,
                    UserName = "manager@konsanad.com",
                    Email = "manager@konsanad.com",
                    // ... باقي الـ properties
                };

                var result = await userManager.CreateAsync(manager, "Manager@123");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(manager, "Manager"); // ✅ Assign Role
                }
            }
        }
    }
}
