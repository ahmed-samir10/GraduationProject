using System.Threading.Tasks;
using KonSanad.Core.Dtos;
using KonSanad.Core.IRepositery;
using KonSanad.Repository.Entities;

namespace KonSanad.Core.IRepositery
{
    public interface IHelpRequestService
    {
       /* Task<HelpRequestResponseDto> RequestExistingProductAsync(int id, ExistingProductRequestDto request);
        Task<HelpRequestResponseDto> RequestNewProductAsync(int id, NewProductRequestDto request);
*/
        //new delete 
        Task<string> DeleteItemAsync(string userId, int orderItemId);
        //new update
        Task< string> UpdateHelpOrderQuantityAsync(
       int helpOrderId,
       int newQuantity);
        Task<string> DeleteHelpRequestAsync(int itemid, string userId);
        Task<string> UpdateHelpRequestAsync(
    int itemId,
    string userId,
    int amount);



        Task<string> JoinCampaignAsync(int id, JoinCampaignRequestDto request);
        Task<IEnumerable<CampaignDto>> GetJoinedCampaignsAsync(string userId);
        Task<IEnumerable<BeneficiaryJoinedCampaignDto>> GetJoinedCampaignsWithStatusAsync(string userId);

        Task<IEnumerable<object>> GetMyHelpRequestsAsync(string userId, int pageNumber, int pageSize);

        Task<GetProfileDto> GetProfileAsync(string userId);
        Task<bool> UpdateProfileAsync(string userId, UpdateProfileDto dto);


    }
}