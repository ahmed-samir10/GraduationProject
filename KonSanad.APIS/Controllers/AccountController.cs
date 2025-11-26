using AutoMapper;
using KonSanad.APIS.DTOs;
using KonSanad.Core.Dtos;
using KonSanad.Core.Dtos.KonSanad.APIS.DTOs;
using KonSanad.Core.Entity.Identity;
using KonSanad.Core.IRepositery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace KonSanad.APIS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ITokenService _tokenService;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IMapper _mapper;

        public AccountController(UserManager<AppUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ITokenService tokenService,
            SignInManager<AppUser> signInManager,
            IMapper mapper)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _tokenService = tokenService;
            _signInManager = signInManager;
            _mapper = mapper;
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
        {
            var user = await _userManager.FindByEmailAsync(loginDto.Email);
            if (user == null)
                return Unauthorized(new ApiResponse(401, "Invalid email or password"));

            var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);
            if (!result.Succeeded)
                return Unauthorized(new ApiResponse(401, "Invalid email or password"));

            var token = await _tokenService.CreateToken(user, _userManager);
            var roles = (await _userManager.GetRolesAsync(user)).ToArray();

            var userDto = new UserDto
            {
                Email = user.Email,
                DisplayName = user.FullName,
                Token = token,
                Roles = roles
            };

            return Ok(userDto);
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
        {
            if (await _userManager.FindByEmailAsync(registerDto.Email) != null)
                return BadRequest(new ApiValidationErrorResponse { Errors = new[] { "Email address already in use." } });

            var allowed = new[] { "Volunteer", "Beneficiary", "Donor" };
            var role = registerDto.Role?.Trim();
            if (string.IsNullOrEmpty(role) || !allowed.Contains(role))
                return BadRequest(new ApiValidationErrorResponse { Errors = new[] { "Role must be one of: Volunteer, Beneficiary, Donor" } });

            // Ensure role exists
            if (!await _roleManager.RoleExistsAsync(role))
            {
                var roleCreate = await _roleManager.CreateAsync(new IdentityRole(role));
                if (!roleCreate.Succeeded)
                {
                    var errors = roleCreate.Errors.Select(e => e.Description).ToArray();
                    return BadRequest(new ApiValidationErrorResponse { Errors = errors });
                }
            }

            var user = _mapper.Map<AppUser>(registerDto);
            user.UserName = registerDto.Email;

            var result = await _userManager.CreateAsync(user, registerDto.Password);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToArray();
                return BadRequest(new ApiValidationErrorResponse { Errors = errors });
            }

            var addRoleResult = await _userManager.AddToRoleAsync(user, role);
            if (!addRoleResult.Succeeded)
            {
                var errors = addRoleResult.Errors.Select(e => e.Description).ToArray();
                return BadRequest(new ApiValidationErrorResponse { Errors = errors });
            }

            var token = await _tokenService.CreateToken(user, _userManager);
            var roles = (await _userManager.GetRolesAsync(user)).ToArray();

            var userDto = new UserDto
            {
                Email = user.Email,
                DisplayName = user.FullName,
                Token = token,
                Roles = roles
            };

            return Ok(userDto);
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult<UserDto>> GetCurrentUser()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var user = await _userManager.FindByEmailAsync(email);

            var token = await _tokenService.CreateToken(user, _userManager);
            var roles = (await _userManager.GetRolesAsync(user)).ToArray();

            return Ok(new UserDto
            {
                Email = user.Email,
                DisplayName = user.FullName,
                Token = token,
                Roles = roles
            });
        }

        [HttpGet("emailexists")]
        public async Task<ActionResult<bool>> CheckEmailExistsAsync([FromQuery] string email)
        {
            return await _userManager.FindByEmailAsync(email) != null;
        }
    }
}
