using AutoMapper;
using KonSanad.APIS.DTOs;
using KonSanad.Core.Dtos;
using KonSanad.Core.Dtos.KonSanad.APIS.DTOs;
using KonSanad.Core.Entities; // ← هنا Donor و Beneficiary و Volunteer
using KonSanad.Core.Entity.Identity;
using KonSanad.Core.IRepositery;
using KonSanad.Repository.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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
        private readonly IUnitOfWork _unitOfWork; // مهم جدًا عشان نضيف Donor/Beneficiary

        public AccountController(
            UserManager<AppUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ITokenService tokenService,
            SignInManager<AppUser> signInManager,
            IMapper mapper,
            IUnitOfWork unitOfWork)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _tokenService = tokenService;
            _signInManager = signInManager;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
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

            return Ok(await BuildUserResponseAsync(user));
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
        {
            if (await _userManager.FindByEmailAsync(registerDto.Email) != null)
                return BadRequest(new ApiValidationErrorResponse { Errors = new[] { "Email already in use" } });

            var allowedRoles = new[] { "Donor", "Beneficiary", "Volunteer" };
            var role = registerDto.Role?.Trim();
            if (string.IsNullOrWhiteSpace(role) || !allowedRoles.Contains(role, StringComparer.OrdinalIgnoreCase))
                return BadRequest(new ApiValidationErrorResponse { Errors = new[] { "Role must be Donor, Beneficiary, or Volunteer" } });

            if (!await _roleManager.RoleExistsAsync(role))
                await _roleManager.CreateAsync(new IdentityRole(role));

            var user = _mapper.Map<AppUser>(registerDto);
            user.UserName = registerDto.Email;

            var result = await _userManager.CreateAsync(user, registerDto.Password);
            if (!result.Succeeded)
                return BadRequest(new ApiValidationErrorResponse { Errors = result.Errors.Select(e => e.Description).ToArray() });

            var roleResult = await _userManager.AddToRoleAsync(user, role);
            if (!roleResult.Succeeded)
                return BadRequest(new ApiValidationErrorResponse { Errors = roleResult.Errors.Select(e => e.Description).ToArray() });

            // هنا السحر: ننشئ حساب المستفيد أو المتبرع أو المتطوع تلقائيًا
            await CreateProfileBasedOnRole(user, role, registerDto);

            return Ok(await BuildUserResponseAsync(user));
        }

        //[Authorize]
        //[HttpGet("current")]
        //public async Task<ActionResult<UserDto>> GetCurrentUser()
        //{
        //    var email = User.FindFirstValue(ClaimTypes.Email);
        //    var user = await _userManager.FindByEmailAsync(email);
        //    return Ok(await BuildUserResponseAsync(user));
        //}

        //[HttpGet("emailexists")]
        //public async Task<ActionResult<bool>> CheckEmailExists([FromQuery] string email)
        //    => await _userManager.FindByEmailAsync(email) != null;

        // دالة مساعدة عشان نرجع نفس الشكل في Login و Register و CurrentUser
        private async Task<UserDto> BuildUserResponseAsync(AppUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            return new UserDto
            {

                Email = user.Email!,

                FullName = user.FullName ?? "User",
                Role = roles.FirstOrDefault() ?? "User",

                Token = await _tokenService.CreateToken(user, _userManager)
            };
        }

        // دالة جديدة: تنشئ الحساب المناسب (Donor / Beneficiary / Volunteer) تلقائيًا
        private async Task CreateProfileBasedOnRole(AppUser user, string role, RegisterDto registerDto)
        {
            switch (role.ToLower())
            {
                case "donor":
                    var donor = new Donor
                    {
                        AppUserId = user.Id,
                        FullName = registerDto.FullName ?? user.FullName ?? "Donor User",
                        CreatedAt = DateTime.UtcNow,
                        Email = user.Email,
                        Password = user.PasswordHash,
                        IsDeleted = false,
                        Address = registerDto.Address ?? "غير محدد",     // مهم
                        Phone = registerDto.Phone ?? "غير محدد",         // مهم
                        NationalId = registerDto.NationalId ?? "غير محدد" // مهم
                    };
                    await _unitOfWork.Repository<Donor, int>().AddAsync(donor);
                    break;

                case "beneficiary":
                    var beneficiary = new Beneficiary
                    {
                        Email = user.Email,
                        Pssword = user.PasswordHash,
                        AppUserId = user.Id,
                     
                        CreatedAt = DateTime.UtcNow,
                        IsDeleted = false,
                        Address = registerDto.Address ?? "غير محدد",     // مهم
                        Phone = registerDto.Phone ?? "غير محدد",         // مهم
                        NationalId = registerDto.NationalId ?? "غير محدد", // مهم
                        // أضف الحقول اللي عايزها: Phone, Address, Needs, etc.
                        FullName = registerDto.FullName?.Trim()
                    };
                    await _unitOfWork.Repository<Beneficiary, int>().AddAsync(beneficiary);
                    break;

                case "volunteer":
                    var volunteer = new Volunteer
                    {
                        AppUserId = user.Id,
                        FullName = registerDto.FullName ?? user.FullName ?? "volunteer User",
                        CreatedAt = DateTime.UtcNow,
                        IsDeleted = false,
                        Address = registerDto.Address ?? "غير محدد",     // مهم
                        Phone = registerDto.Phone ?? "غير محدد",         // مهم
                        NationalId = registerDto.NationalId ?? "غير محدد" // مهم
                    };
                    await _unitOfWork.Repository<Volunteer, int>().AddAsync(volunteer);
                    break;
            }

            await _unitOfWork.Complete();
        }
    }
}