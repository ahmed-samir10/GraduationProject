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
            var normalizedEmail = userManager.NormalizeEmail("manager@konsanad.com");

            // نتجنب تحميل AppUser بالكامل لأن بعض السجلات القديمة قد تحتوي قيماً NULL
            if (!await userManager.Users.AnyAsync(u => u.NormalizedEmail == normalizedEmail))
            {
                var manager = new AppUser
                {
                    FirstName = "Manager",
                    LastName = "KonSanad",
                    FullName = "Manager KonSanad",
                    Phone = "01000000000",
                    PhoneNumber = "01000000000",
                    Address = "Cairo, Egypt",
                    NationalId = "00000000000000",
                    EmailConfirmed = true,
                    UserName = "manager@konsanad.com",
                    Email = "manager@konsanad.com",
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
