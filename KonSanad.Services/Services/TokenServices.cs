
using Microsoft.AspNetCore.Identity;

namespace KonSanad.Services.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;
        

        public TokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

 
        public async Task<string> GenerateJwtToken(AppUser user, IList<string> roles , UserManager<AppUser> userManager)
        {
            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim("Id", user.Id),
            //new Claim(JwtRegisteredClaimNames.Sub, user.Email),
           new Claim("Email", user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("Name", user.UserName ?? "")
        };
            var userRoles = await userManager.GetRolesAsync(user);

            foreach (var role in userRoles)
                claims.Add(new Claim(ClaimTypes.Role, role.ToString()));


           // claims.AddRange(roles.Select(role => new Claim("Role", role)));


            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(234),
                signingCredentials: creds
            );
           
            return new JwtSecurityTokenHandler().WriteToken(token);
        }


    }
}