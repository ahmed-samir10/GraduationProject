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
        // ----------------------------------------------------------
        public async Task<IEnumerable<MissionDto>> GetAllMissionsAsync()
        {
            var dtos = await _context.Missions
                .AsNoTracking()
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
                    MissionStatus = m.MissionStatus
                })
                .ToListAsync();

            return dtos;
        }
        // ----------------------------------------------------------
    


        // Method to start a mission for a volunteer
        // ----------------------------------------------------------
        public async Task StartMissionAsync(int volunteerId, int missionId)
        {
            var existing = await _unitOfWork
             .Repository<MissionVolunteers, int>()
             .FindAllAsync(x => x.MissionId == missionId);

            if (existing.Any())
                throw new Exception("Mission already started");

            var MissionVolunteer = new MissionVolunteers
            {
                VolunteerId = volunteerId,
                MissionId = missionId,
                CreatedAt = DateTime.UtcNow,
                MissionStatus = MissionStatus.InProgress
            };

            await _unitOfWork.Repository<MissionVolunteers, int>()
                .AddAsync(MissionVolunteer);

            await _unitOfWork.Complete();
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

            if (volunteer is null) throw new ArgumentException("Volunteer not found");



            // Validate volunteer-mission relation exists Or not
            var vmRepo = _unitOfWork.Repository<MissionVolunteers, int>();
            var volunteerMission = await vmRepo.FirstAsync(vm => vm.VolunteerId == volunteerId && vm.MissionId == missionId);

            if (volunteerMission is null)
                throw new ArgumentException("VolunteerMission not found for given volunteer and mission");



            // Optionally validate mission exists and dates (if needed)
            var mission = await _unitOfWork.Repository<Mission, int>().FirstAsync(m => m.Id == missionId);
            if (mission is null)
                throw new ArgumentException("Mission not found");



            // If mission has start/end dates, you can prevent reporting outside mission window
            if (mission.StartDate.HasValue && mission.EndDate.HasValue)
            {
                var now = DateTime.UtcNow;
                if (now < mission.StartDate.Value || now > mission.EndDate.Value)
                {
                    throw new ArgumentException("Cannot submit report outside mission active period");
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

            vmRepo.Update(volunteerMission);
            volunteerMission.MissionStatus = MissionStatus.Completed;
            volunteerMission.Status ="Completed";
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
                throw new ArgumentException("Volunteer not found");

            // تأكد الـ campaign موجودة
            var campaign = await _unitOfWork
                .Repository<Campaign, int>()
                .FirstOrDefaultAsync(c => c.Id == campaignId);

            if (campaign is null)
                throw new ArgumentException("Campaign not found");

            // تأكد الـ volunteer مش اتضمّن قبل
            var existing = await _unitOfWork
                .Repository<CampaignVolunteer, int>()
                .FindAllAsync(cv => cv.VolunteerId == volunteerId && cv.CampaignId == campaignId);

            if (existing.Any())
                throw new ArgumentException("Volunteer already joined this campaign");

            // اضيف الـ volunteer للـ campaign
            var campaignVolunteer = new CampaignVolunteer
            {
                VolunteerId = volunteerId,
                CampaignId = campaignId,
                JoinedDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Status = "Pending"
            };

            await _unitOfWork
                .Repository<CampaignVolunteer, int>()
                .AddAsync(campaignVolunteer);

            await _unitOfWork.Complete();

            return "Joined campaign successfully";
        }









    }
}