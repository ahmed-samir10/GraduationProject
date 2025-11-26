using KonSanad.Core.Entity.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;

namespace KonSanad.APIS.Extensions
{
    public static class UserManagerExtensions
    {
        // used by AccountController: FindByEmailWithAddressAsync(User)
        public static async Task<AppUser?> FindByEmailWithAddressAsync(this UserManager<AppUser> userManager, ClaimsPrincipal user)
        {
            var email = user.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(email)) return null;

            return await userManager.Users
          
                .SingleOrDefaultAsync(u => u.Email == email);
        }
    }
}
