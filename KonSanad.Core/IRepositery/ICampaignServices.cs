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
        Task<string> CreateAsync(CampaignCreateDto createDto);
        Task<string> UpdateAsync(int id, CampaignCreateDto updateDto);
        Task<string> DeleteAsync(int id);
        Task<List<CampaignMangerDto>> GetAllCampaignMAngerAsync(int pageNumber, int pageSize);
    }
}