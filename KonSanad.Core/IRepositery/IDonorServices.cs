using KonSanad.Core.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KonSanad.Core.IRepositery
{
   public interface IDonorServices
    {
        Task<List<SupplyDto>> GetLowStockSuppliesAsync();
        Task<DonationResponseCashDto> CreateDonationCashAsync(
           CreateDonationCashRequestDto request, int donorId);
        Task<DonationResponseItemDto> CreateDonationItemAsync(
                   CreateDonationItemRequestDto request, int donorId);
        Task<GetProfileDto> GetProfileAsync(string userId);
        Task<object> GetMyDonationsAsync(string userId);
        Task<IEnumerable<CampaignDto>> GetCampaignsDonatedByAsync(string userId);

    }
}
