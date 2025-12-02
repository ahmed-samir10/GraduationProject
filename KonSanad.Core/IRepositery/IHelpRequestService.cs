using KonSanad.Core.Dtos;
using KonSanad.Core.IRepositery;



using System.Threading.Tasks;

namespace KonSanad.Core.IRepositery
{
    public interface IHelpRequestService
    {
        Task<HelpRequestResponseDto> RequestExistingProductAsync(int id, ExistingProductRequestDto request);
        Task<HelpRequestResponseDto> RequestNewProductAsync(int id, NewProductRequestDto request);
        Task<string> JoinCampaignAsync(int id, JoinCampaignRequestDto request);
    }
}