
using KonSanad.Core.Entity.Enums;

namespace KonSanad.Services.Services
{
    public class CampaignService : ICampaignService
    {
        private readonly KonSanadDbContext _db;
        private readonly IMapper _mapper;
        private readonly INotificationService _notificationService;

        public CampaignService(KonSanadDbContext db, IMapper mapper, INotificationService notificationService)
        {
            _db = db;
            _mapper = mapper;
            _notificationService = notificationService;
        }
        public async Task<List<CampaignDto>> GetAllAsync(int pageNumber, int pageSize)
        {
            var campaigns = await _db.Campaigns
                .Include(c => c.Category)
                .Include(c => c.Manager)
                .Include(c => c.DonationOrders).ThenInclude(d => d.CashDonation)
                .Include(c => c.DonationOrders).ThenInclude(d => d.OrderItems)
                    .ThenInclude(oi => oi.Supply)
                .Include(c => c.CampaignVolunteers)
                .Include(c => c.Missions)
                .Include(c => c.CampaignResults)
                .AsNoTracking()
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var dtos = _mapper.Map<List<CampaignDto>>(campaigns);
            return dtos;
        }


        public async Task<CampaignDto?> GetByIdAsync(int id)
        {
            var campaign = await _db.Campaigns
                .Include(c => c.Category)
                .Include(c => c.Manager)
                .Include(c => c.DonationOrders).ThenInclude(d => d.CashDonation)
                .Include(c => c.DonationOrders).ThenInclude(d => d.OrderItems)
                .Include(c => c.CampaignVolunteers)
                .Include(c => c.Missions)
                .Include(c => c.CampaignResults)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);

            if (campaign == null) return null;


            return _mapper.Map<CampaignDto>(campaign);
        }



        public async Task<CampaignDto> CreateAsync(CampaignCreateDto createDto)
        {
            var entity = _mapper.Map<Campaign>(createDto);
            _db.Campaigns.Add(entity);
            await _db.SaveChangesAsync();

            var notification = new NotificationMessageDto
            {
                Type = NotificationType.CampaignCreated,
                Title = "حملة جديدة",
                Message = $"تمت إضافة حملة جديدة باسم {entity.Title}.",
                EntityType = nameof(Campaign),
                EntityId = entity.Id
            };

            await _notificationService.NotifyRoleAsync("Donor", notification);
            await _notificationService.NotifyRoleAsync("Beneficiary", new NotificationMessageDto
            {
                Type = notification.Type,
                Title = notification.Title,
                Message = notification.Message,
                EntityType = notification.EntityType,
                EntityId = notification.EntityId
            });

            // reload with includes for mapping presentation fields
            return await GetByIdAsync(entity.Id) ?? _mapper.Map<CampaignDto>(entity);
        }

        public async Task<bool> UpdateAsync(int id, CampaignCreateDto updateDto)
        {
            var entity = await _db.Campaigns.FirstOrDefaultAsync(c => c.Id == id);
            if (entity == null) return false;

            // Map allowed fields from DTO to entity (do not overwrite collections)
            _mapper.Map(updateDto, entity);

            _db.Campaigns.Update(entity);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _db.Campaigns.FirstOrDefaultAsync(c => c.Id == id);
            if (entity == null) return false;

            _db.Campaigns.Remove(entity);
            await _db.SaveChangesAsync();
            return true;
        }

       

        public async Task<List<CampaignBeneficiaryDto>> GetAllCampaignBeneficiaryAsync(int pageNumber, int pageSize)
        {
            var campaigns = await _db.Campaigns
                .Include(c => c.Category)
                 .Include(c => c.BeneficiaryCampaigns) //new
                .Include(c => c.Manager)
                .Include(c => c.DonationOrders).ThenInclude(d => d.CashDonation)
                .Include(c => c.DonationOrders).ThenInclude(d => d.OrderItems)
                    .ThenInclude(oi => oi.Supply)
                .Include(c => c.CampaignVolunteers)
                .Include(c => c.Missions)
                .Include(c => c.CampaignResults)
                .AsNoTracking()
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
    }
}
