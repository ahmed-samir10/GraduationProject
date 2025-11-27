using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KonSanad.Core.Dtos;
using KonSanad.Core.IRepositery;
using KonSanad.Repository.Data.Persistence;
using KonSanad.Repository.Entities;
using Microsoft.EntityFrameworkCore;
using AutoMapper;

namespace KonSanad.Services.Services
{
    public class CampaignService : ICampaignService
    {
        private readonly KonSanadDbContext _db;
        private readonly IMapper _mapper;

        public CampaignService(KonSanadDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<List<CampaignDto>> GetAllAsync()
        {
            var campaigns = await _db.Campaigns
                .Include(c => c.Category)
                .Include(c => c.Manager)
                .Include(c => c.DonationOrders).ThenInclude(d => d.CashDonation)
                .Include(c => c.DonationOrders).ThenInclude(d => d.OrderItems)
                .Include(c => c.CampaignVolunteers)
                .Include(c => c.Missions)
                .Include(c => c.CampaignResults)
                .AsNoTracking()
                .ToListAsync();

            return _mapper.Map<List<CampaignDto>>(campaigns);
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

       
    }
}