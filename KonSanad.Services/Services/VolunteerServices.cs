using KonSanad.Core.Entities;
using KonSanad.Core.Entity.Enums;
using Microsoft.AspNetCore.Http.HttpResults;

namespace KonSanad.Services.Services
{
    public class VolunteerService : IVolunteerService
    {
        private readonly KonSanadDbContext _context;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFileService _fileService;
        private readonly UserManager<AppUser> _userManager;

        public VolunteerService(KonSanadDbContext context , IUnitOfWork unitOfWork , IFileService fileService , UserManager<AppUser> userManager)
        {
            _context = context;
            _unitOfWork = unitOfWork;
            _fileService = fileService;
            _userManager = userManager;
        }


        // Method to get all missions with beneficiary details
        public async Task<IEnumerable<MissionDto>> GetAllMissionsAsync(int volunteerId)
        {
            var volunteerMissions = await _context.MissionVolunteers
                .Where(mv => mv.VolunteerId == volunteerId)
                .Select(mv => new { mv.MissionId, mv.MissionStatus })
                .ToListAsync();

            var volunteerMissionDict = volunteerMissions
                .ToDictionary(mv => mv.MissionId, mv => mv.MissionStatus);

            var missions = await _context.Missions
                .AsNoTracking()
                .Where(m => m.MissionStatus == MissionStatus.New
                         || volunteerMissionDict.Keys.Contains(m.Id))
                .Select(m => new MissionDto
                {
                    Id = m.Id,
                    Title = m.HelpOrder != null && m.HelpOrder.Beneficiary != null
                        ? m.HelpOrder.Beneficiary.FullName
                        : (m.Title ?? string.Empty),
                    PhoneNumber = m.HelpOrder != null && m.HelpOrder.Beneficiary != null
                        ? m.HelpOrder.Beneficiary.Phone
                        : (m.PhoneNumber ?? string.Empty),
                    Address = m.HelpOrder != null && m.HelpOrder.Beneficiary != null
                        ? m.HelpOrder.Beneficiary.Address
                        : (m.Address ?? string.Empty),
                    Description = "مطلوب زياره ميدانيه",
                    StartDate = m.CreatedAt,
                    MissionStatus = m.MissionStatus // هنعدلها بعدين
                })
                .ToListAsync();

            // ✅ استبدل الـ Status بتاع المهام المشتركة بالـ Status من MissionVolunteers
            foreach (var mission in missions)
            {
                if (volunteerMissionDict.TryGetValue(mission.Id, out var volunteerStatus))
                {
                    mission.MissionStatus = (MissionStatus)(int)volunteerStatus;
                }
            }

            return missions;
        }

        // Method to start a mission for a volunteer
        // ----------------------------------------------------------
        public async Task<string> StartMissionAsync(int volunteerId, int missionId)
        {
            var existing = await _unitOfWork
             .Repository<MissionVolunteers, int>()
             .FindAllAsync(x => x.MissionId == missionId&& x.VolunteerId == volunteerId);

            if (existing.Any())
               return "تم تقديم طلب لهذه المهمة بالفعل";

            var mission = await _unitOfWork
       .Repository<Mission, int>()
       .GetByIdAsync(missionId);

            if (mission == null)
                return "المهمة غير موجودة";

            // 2️⃣ غيّر حالة المهمة
            mission.MissionStatus = MissionStatus.New;

            var MissionVolunteer = new MissionVolunteers
            {
                VolunteerId = volunteerId,
                MissionId = missionId,
                CreatedAt = DateTime.UtcNow,
                MissionStatus =VolunteerMissionStatus.InReview,
            };

            await _unitOfWork.Repository<MissionVolunteers, int>()
                .AddAsync(MissionVolunteer);


            await _unitOfWork.Complete();
            return "تم استقبال طلبك";
        }
        // ----------------------------------------------------------

        public async Task<IEnumerable<VolunteerMissionDto>> GetMissionsByVolunteerId(int volunteerId)
        {
            var missions = await _unitOfWork
                .Repository<MissionVolunteers, int>()
                .FindAllAsync(
                    predicate: vm => vm.VolunteerId == volunteerId,
                    include: q => q.Include(vm => vm.Mission)
                );

            if (!missions.Any())
                throw new ArgumentException("No missions found for this volunteer");

            return missions.Select(vm => new VolunteerMissionDto
            {
                Id = vm.Id,
                MissionId = vm.MissionId,
                Name = vm.Mission.Title,
                MissionStatus = vm.MissionStatus,
                CreatedAt = vm.CreatedAt,
                Description = "مطلوب زياره ميدانيه",
                ResultMission = vm.ResultMission,
                Notes = vm.Notes
            });
        }
        // ----------------------------------------------------------

        // Method to submit a mission report by a volunteer
        // ----------------------------------------------------------
        public async Task<string> SubmitMissionReportAsync(int volunteerId, int missionId, IFormFile file, string? notes = null)
        {
            // Validate volunteer exists
            var volunteer = await _unitOfWork.Repository<Volunteer, int>().FirstOrDefaultAsync(v => v.Id == volunteerId);

            if (volunteer is null) return "المتطوع غير موجود";



            // Validate volunteer-mission relation exists Or not
            var vmRepo = _unitOfWork.Repository<MissionVolunteers, int>();
            var volunteerMission = await vmRepo.FirstAsync(vm => vm.VolunteerId == volunteerId && vm.MissionId == missionId);

            if (volunteerMission is null)
              return "المتطوع غير مرتبط بهذه المهمة";



            // Optionally validate mission exists and dates (if needed)
            var mission = await _unitOfWork.Repository<Mission, int>().FirstAsync(m => m.Id == missionId);
            if (mission == null)
                return "المهمة غير موجودة";

            // 2️⃣ غيّر حالة المهمة
            mission.MissionStatus = MissionStatus.Completed;



            // If mission has start/end dates, you can prevent reporting outside mission window
            if (mission.StartDate.HasValue && mission.EndDate.HasValue)
            {
                var now = DateTime.UtcNow;
                if (now < mission.StartDate.Value || now > mission.EndDate.Value)
                {
                   return "لا يمكن تقديم التقرير خارج فترة المهمة المحددة";
                }
            }


            // Save file
            var relativeUrl = await _fileService.SaveFileAsync(file, "uploads");


            // Store the result (for now store a single URL).
            // If multiple files are needed consider storing JSON or separate table.
            volunteerMission.ResultMission = relativeUrl;
            if (!string.IsNullOrEmpty(notes))
            {
                volunteerMission.Notes = notes;
            }

            //vmRepo.Update(volunteerMission);
            volunteerMission.MissionStatus =VolunteerMissionStatus.Completed;
            //volunteerMission.Status ="Completed";
            vmRepo.Update(volunteerMission);
            await _unitOfWork.Complete();

            return relativeUrl;
        }
        // ----------------------------------------------------------



        // Method to get volunteer profile by user ID
        // ----------------------------------------------------------
        public async Task<GetProfileDto> GetProfileAsync(string userId)
        {
            // 1) استخدم UserManager للحصول على AppUser
            var appUser = await _userManager.FindByIdAsync(userId);

            if (appUser is null)
                throw new Exception("User not found");

            // 2) ثم احصل على Donor
            var volunteer = await _unitOfWork.Repository<Volunteer, int>()
                .FirstOrDefaultAsync(v => v.AppUserId == userId);

            if (volunteer is null)
                throw new Exception("Volunteer not found");

            // 3) Map to DTO
            var profileDto = new GetProfileDto
            {
                FullName = volunteer.FullName ?? string.Empty,
                NationalId = volunteer.NationalId ?? string.Empty,
                Address = volunteer.Address ?? string.Empty,
                PhoneNumber = appUser.PhoneNumber ?? string.Empty,
                Email = appUser.Email ?? string.Empty
            };
            return profileDto;
        }
        // ----------------------------------------------------------



        // Method to update volunteer profile
        // ----------------------------------------------------------
        public async Task<bool> UpdateProfileAsync(string userId, UpdateProfileDto dto)
        {
            // 1) Get Volunteer by AppUserId
            var volunteerRebo = _unitOfWork.Repository<Volunteer, int>();

            // 2) Get Volunteer
            var volunteer = await volunteerRebo.FirstOrDefaultAsync(
                d => d.AppUserId == userId
            );

            // 3) Validate Volunteer exists
            if (volunteer is null)
                throw new Exception("Volunteer not found");

            // 4) Update fields if provided
            if (!string.IsNullOrWhiteSpace(dto.FullName) && !(dto.FullName == "string"))
                volunteer.FullName = dto.FullName;

            if (!string.IsNullOrWhiteSpace(dto.NationalId) && !(dto.NationalId == "string"))
                volunteer.NationalId = dto.NationalId;

            if (!string.IsNullOrWhiteSpace(dto.PhoneNumber) && !(dto.PhoneNumber == "string"))
                volunteer.Phone = dto.PhoneNumber;

            if (!string.IsNullOrWhiteSpace(dto.Address) && !(dto.Address == "string"))
                volunteer.Address = dto.Address;


            // 5) If Email is provided, update Identity user email
            if (!string.IsNullOrWhiteSpace(dto.Email) && !(dto.Email == "string"))
            {
                // Get AppUser
                var appUser = await _userManager.FindByIdAsync(userId);
                if (appUser is null)
                    throw new Exception("App user not found");

                // Update email if different
                if (!string.Equals(appUser.Email, dto.Email, StringComparison.OrdinalIgnoreCase))
                {
                    appUser.Email = dto.Email;

                    // Optionally update UserName if your system uses email as username
                    var result = await _userManager.UpdateAsync(appUser);
                    if (!result.Succeeded)
                    {
                        var errors = string.Join(';', result.Errors.Select(e => e.Description));
                        throw new Exception($"Failed to update identity user: {errors}");
                    }
                }

                // Keep volunteer.Email in sync 
                volunteer.Email = dto.Email;
            }


            // 6) If PhoneNumber is provided, update Identity user phone number
            if (!string.IsNullOrWhiteSpace(dto.PhoneNumber) && !(dto.PhoneNumber == "string"))
            {
                // Get AppUser
                var appUser = await _userManager.FindByIdAsync(userId);
                if (appUser is null)
                    throw new Exception("App user not found");

                // Update phone number if different
                if (!string.Equals(appUser.PhoneNumber, dto.PhoneNumber, StringComparison.OrdinalIgnoreCase))
                {
                    appUser.PhoneNumber = dto.PhoneNumber;

                    // Update phone number in Identity system
                    var result = await _userManager.SetPhoneNumberAsync(appUser, dto.PhoneNumber);
                    if (!result.Succeeded)
                    {
                        var errors = string.Join(';', result.Errors.Select(e => e.Description));
                        throw new Exception($"Failed to update phone number: {errors}");
                    }
                }

                // Keep Volunteer.Phone in sync
                volunteer.Phone = dto.PhoneNumber;
            }

            // 7) Save changes
            _unitOfWork.Repository<Volunteer, int>().Update(volunteer);
            await _unitOfWork.Complete();

            return true;
        }
        // ----------------------------------------------------------



        public async Task<string> JoinCampaignAsync(int volunteerId, int campaignId)
        {
            // تأكد الـ volunteer موجود
            var volunteer = await _unitOfWork
                .Repository<Volunteer, int>()
                .FirstOrDefaultAsync(v => v.Id == volunteerId);

            if (volunteer is null)
                return "المطتوع غير موجود";

            // تأكد الـ campaign موجودة
            var campaign = await _unitOfWork
                .Repository<Campaign, int>()
                .FirstOrDefaultAsync(c => c.Id == campaignId);

            if (campaign is null)
                return "الحملة غير موجودة";
            // تأكد الـ volunteer مش اتضمّن قبل
            var existing = await _unitOfWork
                .Repository<CampaignVolunteer, int>()
                .FindAllAsync(cv => cv.VolunteerId == volunteerId && cv.CampaignId == campaignId);

            if (existing.Any())
                return "تم تقديم طلب لهذه الحملة بالفعل";

            // اضيف الـ volunteer للـ campaign
            var campaignVolunteer = new CampaignVolunteer
            {
                VolunteerId = volunteerId,
                CampaignId = campaignId,
                JoinedDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Status = RequestedStatus.Pending
            };

            await _unitOfWork
                .Repository<CampaignVolunteer, int>()
                .AddAsync(campaignVolunteer);

            await _unitOfWork.Complete();

            return "تم إرسال الطلب بنجاح";
        }


        public async Task<IEnumerable<CampaignDto>> GetJoinedCampaignsAsync(string userId)
        {
            // 1) نجيب المستفيد حسب AppUserId
            var beneficiary = await _unitOfWork.Repository<Volunteer, int>()
                .FirstOrDefaultAsync(b => b.AppUserId == userId);

            if (beneficiary == null)
                return null;
            // 2) نجيب العلاقات بين المستفيد والحملات مع Include
            var joinedCampaigns = await _unitOfWork.Repository<CampaignVolunteer, int>()
    .FindAllAsync(
        bc => bc.VolunteerId == beneficiary.Id,
        include: q => q
            .Include(x => x.Campaign)
                .ThenInclude(c => c.DonationOrders)  // 👈 أضف هذا
            .Include(x => x.Campaign)
                .ThenInclude(c => c.Category)        // 👈 وهذا للـ CategoryName
    );

            // 3) نتأكد إن Campaign موجود
            var campaignEntities = joinedCampaigns
                .Where(j => j.Campaign != null)
                .Select(j => j.Campaign)
                .ToList();

            // 4) Map
            var campaignDtos = campaignEntities.Select(c => new CampaignDto
            {
                Id = c.Id,
                Title = c.Title ?? string.Empty,
                PictureUrl = c.PictureUrl ?? string.Empty,
                ShortDescription = string.IsNullOrWhiteSpace(c.Description) ? string.Empty :
         c.Description.Length > 180 ? c.Description.Substring(0, 180).Trim() + "…" : c.Description,
                CategoryName = c.Category?.Name ?? string.Empty,
                ProgressPercentage = CampaignMetrics.CalculateProgress(c),
                Status = CampaignMetrics.CalculateStatus(c),
                DonorCount = CampaignMetrics.CalculateDonorCount(c),
                CreateAt = c.CreatedAt
            }).ToList();
            return campaignDtos;
        }




        public async Task<string> UpdateMissionReportAsync(
      int volunteerId,
      int missionId,
      IFormFile? newFile,
      string? notes = null)
        {
            var vmRepo = _unitOfWork.Repository<MissionVolunteers, int>();

            var volunteerMission = await vmRepo.FirstOrDefaultAsync(
                vm => vm.VolunteerId == volunteerId &&
                      vm.MissionId == missionId);

            if (volunteerMission == null)
                return "المهمة غير موجودة لهذا المتطوع";

          
            if (newFile != null)
            {
                if (!string.IsNullOrEmpty(volunteerMission.ResultMission))
                {
                    await _fileService.DeleteFileAsync(volunteerMission.ResultMission);
                }

                var newUrl = await _fileService.SaveFileAsync(newFile, "uploads");
                volunteerMission.ResultMission = newUrl;
            }

            // ✏️ تحديث الملاحظات
            if (!string.IsNullOrEmpty(notes))
            {
                volunteerMission.Notes = notes;
            }

            volunteerMission.UpdatedAt = DateTime.UtcNow;

            vmRepo.Update(volunteerMission);
            await _unitOfWork.Complete();

            return "تم تحديث التقرير بنجاح";
        }

        public async Task<string> DeleteMissionReportAsync(int volunteerId, int missionId)
        {
            var vmRepo = _unitOfWork.Repository<MissionVolunteers, int>();

            var volunteerMission = await vmRepo.FirstOrDefaultAsync(
                vm => vm.VolunteerId == volunteerId &&
                      vm.MissionId == missionId);

            if (volunteerMission == null)
                return "المهمة غير موجودة لهذا المتطوع";

            // 🗑️ حذف الملف من السيرفر لو موجود
            if (!string.IsNullOrEmpty(volunteerMission.ResultMission))
            {
                await _fileService.DeleteFileAsync(volunteerMission.ResultMission);
            }

            // ✅ حذف الصف نفسه من DB
            vmRepo.Delete(volunteerMission);

            var rows = await _unitOfWork.Complete();

            return $" تم الحذف ";
        }




    }
}