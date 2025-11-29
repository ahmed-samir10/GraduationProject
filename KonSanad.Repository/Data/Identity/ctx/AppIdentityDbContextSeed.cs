using KonSanad.Core.Entity.Identity;
using Microsoft.AspNetCore.Identity;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace KonSanad.Repository.Data.Identity.ctx
{
    public class AppIdentityDbContextSeed
    {
        public static async Task SeedUsersAsync(UserManager<AppUser> userManager)
        {
            if (!userManager.Users.Any())
            {
                var user = new AppUser()
                {
                    FirstName = "Ahmed samir",
                 
                    Email = "ahmedSamir97@gmail.com",
                    UserName = "ahmedSamir97@gmail.com", // <-- important
                    PhoneNumber = "01015317037",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(user, "Pa$$w0rd");
                if (!result.Succeeded)
                {
                    var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                    throw new InvalidOperationException($"Seeding user failed: {errors}");
                }
            }
        }
    }
}
