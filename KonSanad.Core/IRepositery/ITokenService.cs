using KonSanad.Core.Entity.Identity;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KonSanad.Core.IRepositery
{
    public interface ITokenService
    {
        //   Task<string> CreateToken(AppUser user, UserManager<AppUser> userManager);
        Task<string> GenerateJwtToken(AppUser user, IList<string> roles, UserManager<AppUser> userManager);
    }
}
