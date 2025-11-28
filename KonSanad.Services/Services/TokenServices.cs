using KonSanad.Core.Entity.Identity;
using KonSanad.Core.IRepositery;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace KonSanad.Services.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;

        public TokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<string> CreateToken(AppUser user, UserManager<AppUser> userManager)
        {
            // جيب الرولات
            var userRoles = await userManager.GetRolesAsync(user);

            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id ?? string.Empty),
               new Claim("Id", user.Id ?? string.Empty),
                 new Claim("fullName", user.FullName ?? "User"),
               new Claim("Email", user.Email ?? string.Empty),
                  new Claim("DisplayName", user.FullName ?? "User"),
                // السطر الصحيح: نحط الرولات كـ array داخل التوكن
            new Claim("role", userRoles.FirstOrDefault() ?? "User"),




                // اختياري: لو عايز role واحد للـ [Authorize(Roles = "...")]
                new Claim(ClaimTypes.Role, userRoles.FirstOrDefault() ?? "User")
            };

            // مهم جدًا: دعم أكتر من رول في الـ Policy والـ Authorize
            foreach (var role in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = _configuration["JWT:Key"]
                     ?? throw new InvalidOperationException("JWT:Key is missing in configuration");

            var authKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));

            var days = _configuration.GetValue<double>("JWT:DurationInDays", 30);

            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:ValidIssuer"],
                audience: _configuration["JWT:ValidAudience"],
                expires: DateTime.UtcNow.AddDays(days),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authKey, SecurityAlgorithms.HmacSha256)
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}