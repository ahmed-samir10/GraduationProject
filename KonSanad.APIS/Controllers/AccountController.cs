using AutoMapper;

using KonSanad.Core.Dtos;
using KonSanad.Core.Dtos.KonSanad.APIS.DTOs;
using KonSanad.Core.Dtos.ManagerDtos;
using KonSanad.Core.Entities; // ← هنا Donor و Beneficiary و Volunteer
using KonSanad.Core.Entity;
using KonSanad.Core.Entity.Enums;
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
        private readonly IUnitOfWork _unitOfWork;
        private readonly INotificationService _notificationService; // مهم جدًا عشان نضيف Donor/Beneficiary

        public AccountController(
            UserManager<AppUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ITokenService tokenService,
            SignInManager<AppUser> signInManager,
            IMapper mapper,
            IUnitOfWork unitOfWork,
            INotificationService notificationService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _tokenService = tokenService;
            _signInManager = signInManager;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _notificationService = notificationService;
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
        {
            var user = await _userManager.FindByEmailAsync(loginDto.Email);
            if (user == null)
                return Ok(new { success = false, data = "  خطا في الايميل او الباسورد" });

            var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);
            if (!result.Succeeded)
                return Ok(new { success = false, data = "  خطا في الايميل او الباسورد" });

            return Ok(await BuildUserResponseAsync(user));
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
        {
            if (await _userManager.FindByEmailAsync(registerDto.Email) != null)
               return Ok(new { success = false, data = "  تم استخدام هذا الايميل من قبل" });

            var allowedRoles = new[] { "Donor", "Beneficiary", "Volunteer" };
            var role = registerDto.Role?.Trim();
            if (string.IsNullOrWhiteSpace(role) || !allowedRoles.Contains(role, StringComparer.OrdinalIgnoreCase))
                return Ok(new { success = false, data = "Role must be Donor, Beneficiary, or Volunteer" });

            if (!await _roleManager.RoleExistsAsync(role))
                await _roleManager.CreateAsync(new IdentityRole(role));

            var user = new AppUser
            {
                Email = registerDto.Email,
                UserName = registerDto.Email,
                PhoneNumber = registerDto.Phone,
                Phone = registerDto.Phone,
                Address = registerDto.Address,
                NationalId = registerDto.NationalId,
            };

            user.FullName = registerDto.FullName;

            var result = await _userManager.CreateAsync(user, registerDto.Password);
            if (!result.Succeeded)
                return BadRequest(new ApiValidationErrorResponse { Errors = result.Errors.Select(e => e.Description).ToArray() });

            var roleResult = await _userManager.AddToRoleAsync(user, role);
            if (!roleResult.Succeeded)
                return BadRequest(new ApiValidationErrorResponse { Errors = roleResult.Errors.Select(e => e.Description).ToArray() });

            await CreateProfileBasedOnRole(user, role, registerDto);

            return Ok(await BuildUserResponseAsync(user));
        }


        private async Task<UserDto> BuildUserResponseAsync(AppUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            return new UserDto
            {

                Email = user.Email!,

                FullName = user.FullName ?? "User",
                Role = roles.FirstOrDefault() ?? "User",

                Token = await _tokenService.GenerateJwtToken(user, roles ,_userManager)
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
                        Email = user.Email,
                        Password = user.PasswordHash,
                        AppUserId = user.Id,
                        Skills = registerDto.Skills ?? "غير محدد", // مهم
                        Availability = registerDto.Availability ?? "غير محدد", // مهم
                        FullName = registerDto.FullName ?? user.FullName ?? "volunteer User",
                        CreatedAt = DateTime.UtcNow,
                        IsDeleted = false,
                        Status = Core.Entity.Enums.RequestedStatus.Pending, // مهم
                        Address = registerDto.Address ?? "غير محدد",     // مهم
                        Phone = registerDto.Phone ?? "غير محدد",         // مهم
                        NationalId = registerDto.NationalId ?? "غير محدد" // مهم
                    };
                    await _unitOfWork.Repository<Volunteer, int>().AddAsync(volunteer);
                    await _notificationService.NotifyManagersAsync(new NotificationMessageDto
                    {
                        Type = NotificationType.MissionRequestSubmitted,
                        Title = "طلب انضام جديد",
                        Message = $"طلب انضام جديد",
                        EntityType = nameof(MissionVolunteers),
                       // EntityId = (int)user.Id
                    });
                    break;
            }

            await _unitOfWork.Complete();
        }

       
        [Authorize(Roles = "Manager")]


        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return BadRequest(new { error = "User id not found in token" });

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return NotFound(new { error = "User not found" });

            return Ok(new
            {
                success = true,
                data = new
                {
                    user.Id,
                    user.FullName,
                    user.Email,
                    user.Phone,
                    user.Address,
                    user.NationalId
                }
            });
        }
        [Authorize(Roles = "Manager")]
        [HttpPut("UpdateProfile")]
        public async Task<IActionResult> UpdateProfile(
    [FromBody] UpdateAdminProfileDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return BadRequest(new { error = "User id not found in token" });

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return NotFound(new { error = "User not found" });
            if (!string.IsNullOrWhiteSpace(dto.FullName) &&
                dto.FullName.Trim().ToLower() != "string")
            {
                user.FullName = dto.FullName;
            }

            if (!string.IsNullOrWhiteSpace(dto.Phone) &&
                dto.Phone.Trim().ToLower() != "string")
            {
                user.Phone = dto.Phone;
                user.PhoneNumber = dto.Phone;
            }

            if (!string.IsNullOrWhiteSpace(dto.Address) &&
                dto.Address.Trim().ToLower() != "string")
            {
                user.Address = dto.Address;
            }

            if (!string.IsNullOrWhiteSpace(dto.NationalId) &&
                dto.NationalId.Trim().ToLower() != "string")
            {
                user.NationalId = dto.NationalId;
            }

            if (!string.IsNullOrWhiteSpace(dto.Email) &&
                dto.Email.Trim().ToLower() != "string")
            {
                var existingUser = await _userManager.FindByEmailAsync(dto.Email);

                if (existingUser != null && existingUser.Id != user.Id)
                    return BadRequest(new { message = "Email already exists" });

                user.Email = dto.Email;
                user.UserName = dto.Email;
            }
            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok(new
            {
                success = true,
                message = "تم التعديل بنجاح"
            });
        }
    }
}