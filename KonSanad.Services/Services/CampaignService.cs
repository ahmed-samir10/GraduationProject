
using KonSanad.Core.Entity.Enums;
using KonSanad.Core.IRepositery;
using KonSanad.Repository.Entities;

namespace KonSanad.Services.Services
{
    public class CampaignService : ICampaignService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly INotificationService _notificationService;
        private readonly IWebHostEnvironment _env;

        public CampaignService( IUnitOfWork unitOfWork, IMapper mapper, INotificationService notificationService,   IWebHostEnvironment env)
        {
             _unitOfWork = unitOfWork;
            _mapper = mapper;
            _notificationService = notificationService;
            _env = env;
        }
        public async Task<List<CampaignDto>> GetAllAsync(int pageNumber, int pageSize)
        {
            // 1️⃣ هات الكامبين + الداتا اللي الحسابات محتاجاها
            var campaigns = await _unitOfWork.Repository<Campaign, int>()
                .GetQueryable()
                .AsNoTracking()
                .Include(c => c.DonationOrders) // عشان DonorCount
                .ThenInclude(d => d.CashDonation)
                .Where(c => !c.IsDeleted)
                .Include(c => c.DonationOrders)
                .ThenInclude(d => d.OrderItems)

                .Include(c => c.Category)

                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // 2️⃣ Map يدوي + الحسابات
            var result = campaigns.Select(c => new CampaignDto
            {
                Id = c.Id,
                Title = c.Title ?? "",
                ShortDescription = c.Description ?? "",
                PictureUrl = c.PictureUrl ?? "",

                CategoryName = c.Category?.Name ?? "",

                // 🔥 الحسابات هنا
                ProgressPercentage = CampaignMetrics.CalculateProgress(c),
                Status = CampaignMetrics.CalculateStatus(c),
                DonorCount = CampaignMetrics.CalculateDonorCount(c)

            }).ToList();

            return result;
        }
        public async Task<CampaignDto?> GetByIdAsync(int id)
        {
            var campaign = await _unitOfWork.Repository<Campaign, int>()
              .GetQueryable()
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);

            if (campaign == null) return null;


            return _mapper.Map<CampaignDto>(campaign);
        }



        public async Task<string> CreateAsync(CampaignCreateDto createDto)
        {
            if (createDto == null)
                throw new ArgumentNullException(nameof(createDto));

            // 1️⃣ Map
            var campaign = _mapper.Map<Campaign>(createDto);

            // 2️⃣ Handle Image Upload
            if (createDto.ImageUrl != null && createDto.ImageUrl.Length > 0)
            {
                var uploadsFolder = Path.Combine(_env.WebRootPath, "Image");

                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var fileName = Guid.NewGuid().ToString() +
                               Path.GetExtension(createDto.ImageUrl.FileName);

                var filePath = Path.Combine(uploadsFolder, fileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                await createDto.ImageUrl.CopyToAsync(stream);

                campaign.PictureUrl = "Image/" + fileName;
            }
            else
            {
                // default image (important عشان تمنع error)
                campaign.PictureUrl = "Image/default.png";
            }

            // 3️⃣ Business Validation
            if (campaign.StartDate > campaign.EndDate)
                throw new Exception("StartDate must be before EndDate");
            campaign.GoalCash = createDto.GoalCash;
           // campaign.GoalSupplies = createDto.GoalSupplies;
            campaign.MaxBeneficiaries = createDto.MaxBenificary;


            // 4️⃣ Add
            await _unitOfWork.Repository<Campaign, int>().AddAsync(campaign);

            // 5️⃣ Save
            await _unitOfWork.Complete();

            var volunteers = await _unitOfWork.Repository<Volunteer , int>()
                .GetAllAsync();

            foreach (var volunteer in volunteers)
            {
                await _notificationService.NotifyUserAsync(
                    volunteer.AppUserId!,
                    new NotificationMessageDto
                    {
                        Title = " تم انشاء حمله جديده",
                        Message = $" تم انشاء حمله جديده برقم {campaign.Id}  بامكانك الانضمام لها",
                        EntityId = campaign.Id,
                        EntityType = nameof(Campaign),
                        CreatedAtUtc = DateTime.UtcNow
                    });
            }

            var Ben = await _unitOfWork.Repository<Beneficiary, int>()
                .GetAllAsync();

            foreach (var b in Ben)
            {
                await _notificationService.NotifyUserAsync(
                    b.AppUserId!,
                    new NotificationMessageDto
                    {
                        Title = " تم انشاء حمله جديده",
                        Message = $" تم انشاء حمله جديده برقم {campaign.Id}  بامكانك الانضمام لها",
                        EntityId = campaign.Id,
                        EntityType = nameof(Campaign),
                        CreatedAtUtc = DateTime.UtcNow
                
                   });
            }
            var donor = await _unitOfWork.Repository<Donor, int>()
           .GetAllAsync();

            foreach (var d in donor)
            {
                await _notificationService.NotifyUserAsync(
                    d.AppUserId!,
                    new NotificationMessageDto
                    {
                        Title = " تم انشاء حمله جديده",
                        Message = $" تم انشاء حمله جديده برقم {campaign.Id}  بامكانك التبرع لها",
                        EntityId = campaign.Id,
                        EntityType = nameof(Campaign),
                        CreatedAtUtc = DateTime.UtcNow
                    });
            }




            return " تم انشاء الحمله بنجاح";
        }

        public async Task<string> UpdateAsync(int id, CampaignCreateDto updateDto)
        {
            var repo = _unitOfWork.Repository<Campaign, int>();

            var entity = await repo.GetByIdAsync(id);
            if (entity == null) return "خطا في التعديل";

            entity.Title = updateDto.Title;
            entity.Description = updateDto.Description;
            entity.GoalCash = updateDto.GoalCash;
           // entity.GoalSupplies = updateDto.GoalSupplies;
            entity.CategoryId = (int)updateDto.CategoryId;
            entity.MaxBeneficiaries = updateDto.MaxBenificary;

            if (updateDto.ImageUrl != null)
            {
                entity.PictureUrl = await SaveImageAsync(updateDto.ImageUrl);
            }

            repo.Update(entity);
            await _unitOfWork.Complete();
            var volunteers = await _unitOfWork.Repository<Volunteer, int>()
           .GetAllAsync();

            foreach (var volunteer in volunteers)
            {
                await _notificationService.NotifyUserAsync(
                    volunteer.AppUserId!,
                    new NotificationMessageDto
                    {
                        Title = " تم تعديل حمله جديده",
                        Message = $" تم نعديل حمله برقم {entity.Id}  بامكانك الاطلاع عليها",
                        EntityId = entity.Id,
                        EntityType = nameof(Campaign),
                        CreatedAtUtc = DateTime.UtcNow
                    });
            }

            var Ben = await _unitOfWork.Repository<Beneficiary, int>()
                .GetAllAsync();

            foreach (var b in Ben)
            {
                await _notificationService.NotifyUserAsync(
                    b.AppUserId!,
                    new NotificationMessageDto
                    {
                        Title = " تم تعديل حمله جديده",
                        Message = $" تم نعديل حمله برقم {entity.Id}  بامكانك الاطلاع عليها",
                        EntityId = entity.Id,
                        EntityType = nameof(Campaign),
                        CreatedAtUtc = DateTime.UtcNow

                    });
            }
            var donor = await _unitOfWork.Repository<Donor, int>()
           .GetAllAsync();

            foreach (var d in donor)
            {
                await _notificationService.NotifyUserAsync(
                    d.AppUserId!,
                    new NotificationMessageDto
                    {
                        Title = " تم تعديل حمله جديده",
                        Message = $" تم نعديل حمله برقم {entity.Id}  بامكانك الاطلاع عليها",
                        EntityId = entity.Id,
                        EntityType = nameof(Campaign),
                        CreatedAtUtc = DateTime.UtcNow
                    });
            }

            return "تم التعديل بنجاح";
        }



        // ========================= DELETE =========================
        public async Task<string> DeleteAsync(int id)
        {
            var repo = _unitOfWork.Repository<Campaign, int>();

            var entity = await repo.GetByIdAsync(id);
            if (entity == null) return "false";

            entity.IsDeleted = true;
            await _unitOfWork.Complete();
            

            return "تم الحذف ";
        }

        // ========================= IMAGE HANDLER =========================
        private async Task<string> SaveImageAsync(IFormFile? file)
        {
            if (file == null || file.Length == 0)
                return "Image/default.png";

            var uploadsFolder = Path.Combine(_env.WebRootPath, "Image");

            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            return "Image/" + fileName;
        }


        public async Task<List<CampaignBeneficiaryDto>> GetAllCampaignBeneficiaryAsync(int pageNumber, int pageSize)
        {
            var campaigns = await _unitOfWork.Repository<Campaign,int>()
               .GetQueryable()
                .AsNoTracking()
                 .Include(c => c.DonationOrders) // عشان DonorCount
                .ThenInclude(d => d.CashDonation)
                .Where(c => !c.IsDeleted)
                .Include(c => c.DonationOrders)
                .ThenInclude(d => d.OrderItems)

                .Include(c => c.Category)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var dtos = campaigns.Select(c => new CampaignBeneficiaryDto
            {
                Id = c.Id,
                Title = c.Title ?? string.Empty,
                ShortDescription = string.IsNullOrWhiteSpace(c.Description) ? string.Empty :
                        (c.Description.Length > 180 ? c.Description.Substring(0, 180).Trim() + "…" : c.Description),
                ProgressPercentage = CampaignMetrics.CalculateProgress(c),
                Status = CampaignMetrics.CalculateStatus(c),
                CampaignStatus = (CampaignStatus)CampaignMetrics.DetermineCampaignStatus(c), // هنا enum مباشرة
                DonorCount = CampaignMetrics.CalculateDonorCount(c),
                CategoryName = c.Category.Name ?? string.Empty,
                 PictureUrl = c.PictureUrl ?? string.Empty,
            }).ToList();
            return dtos;
        }
        public async Task<List<CampaignMangerDto>> GetAllCampaignMAngerAsync(int pageNumber, int pageSize)
        {
            var campaigns =   await _unitOfWork.Repository<Campaign, int>()
               .GetQueryable()
                .AsNoTracking()
                .Include(c => c.Category)
        .Include(c => c.DonationOrders)
            .ThenInclude(d => d.CashDonation)
            .Where(c => !c.IsDeleted)

        .Include(c => c.DonationOrders)
            .ThenInclude(d => d.OrderItems)

        .Include(c => c.BeneficiaryCampaigns)
        .Include(c => c.CampaignVolunteers)

        .Skip((pageNumber - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync();

            var dtos = campaigns.Select(c => new CampaignMangerDto
            {
                Id = c.Id,
               startDate =(DateTime) c.StartDate,
               endDate=(DateTime)c.EndDate,
                
                Title = c.Title ?? string.Empty,
                ShortDescription = string.IsNullOrWhiteSpace(c.Description) ? string.Empty :
                        (c.Description.Length > 180 ? c.Description.Substring(0, 180).Trim() + "…" : c.Description),
                ProgressPercentage = CampaignMetrics.CalculateProgress(c),
                Status = CampaignMetrics.CalculateStatus(c),
                CampaignStatus = (CampaignStatus)CampaignMetrics.DetermineCampaignStatus(c), // هنا enum مباشرة
                DonorCount = CampaignMetrics.CalculateDonorCount(c),
                CategoryName = c.Category.Name ?? string.Empty,
                PictureUrl = c.PictureUrl ?? string.Empty,
                CountBen = CampaignMetrics.CalculateBeneficiaryCount(c) ,
                CountVolunteer = CampaignMetrics.CalculateVolunteerCount(c),
            }).ToList();
            return dtos;
        }
    }

}
