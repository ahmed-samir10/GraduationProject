using System.Collections.Generic;
using System.Threading.Tasks;
using KonSanad.Core.Dtos;

namespace KonSanad.Core.IRepositery
{
    public interface ICampaignService
    {
        Task<List<CampaignDto>> GetAllAsync(int pageNumber, int pageSize);

        Task<List<CampaignBeneficiaryDto>> GetAllCampaignBeneficiaryAsync(int pageNumber, int pageSize);

        Task<CampaignDto?> GetByIdAsync(int id);
        Task<CampaignDto> CreateAsync(CampaignCreateDto createDto);
        Task<bool> UpdateAsync(int id, CampaignCreateDto updateDto);
        Task<bool> DeleteAsync(int id);
    }
}