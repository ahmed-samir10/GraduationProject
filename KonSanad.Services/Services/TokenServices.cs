using KonSanad.Core.Entity.Identity;
using KonSanad.Core.IRepositery;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace KonSanad.Services.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration configuration;

        public TokenService(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public async Task<string> CreateToken(AppUser user, UserManager<AppUser> userManager)
        {
            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id ?? string.Empty),
                new Claim(ClaimTypes.Name, user.UserName ?? user.Email ?? string.Empty),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                new Claim(ClaimTypes.GivenName, user.FullName ?? string.Empty)
            };

            var userRoles = await userManager.GetRolesAsync(user);
            foreach (var role in userRoles)
                authClaims.Add(new Claim(ClaimTypes.Role, role));

            var key = configuration["JWT:Key"] ?? throw new InvalidOperationException("JWT:Key not configured");
            var authKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));

            var days = configuration.GetValue<double>("JWT:DurationInDays", 30);
            var expires = DateTime.UtcNow.AddDays(days);

            var token = new JwtSecurityToken(
                issuer: configuration["JWT:ValidIssuer"],
                audience: configuration["JWT:ValidAudience"],
                expires: expires,
                claims: authClaims,
                signingCredentials: new SigningCredentials(authKey, SecurityAlgorithms.HmacSha256)
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
