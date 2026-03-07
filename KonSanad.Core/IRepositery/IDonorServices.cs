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

        Task<(bool Success, string Message)> UpdateItemQuantityAsync(int orderItemId, int newQuantity);

        Task<(bool Success, string Message)> DeleteItemAsync(int orderItemId);
        Task<List<SupplyDto>> GetLowStockSuppliesAsync(int pageNumber, int pageSize);
        /*        Task<DonationResponseCashDto> CreateDonationCashAsync(
                   CreateDonationCashRequestDto request, int donorId);*/
        /*  Task<DonationResponseItemDto> CreateDonationItemAsync(
                     CreateDonationItemRequestDto request, int donorId);*/
        /*     Task<DonationResponseItemDto> CreateCustomItemDonationAsync(
                 CreateCustomDonationItemRequestDto request, int donorId);*/
        Task<GetProfileDto> GetProfileAsync(string userId);

        Task<bool> DeleteCashDonationAsync(int cashDonationId);
        Task<bool> UpdateCashDonationAmountAsync(int cashDonationId, decimal amount);

        //old
        // Task<IEnumerable<ItemDonationDto>> GetMyItemDonationsAsync(string userId);

        //new
        Task<IEnumerable<MyOrderItemDto>> GetMyDonatedOrderItemsAsync(string userId);
       Task<IReadOnlyList<CashDonationDto>> GetMyCashDonationsAsyncc(string userId);
        //-----------------------------------------------------------------------------
       // Task<IEnumerable<CashDonationDto>> GetMyCashDonationsAsyncc(string userId);
       // Task<object> GetMyDonationsAsync(string userId);
        Task<IEnumerable<CampaignDto>> GetCampaignsDonatedByAsync(string userId);
        Task<IEnumerable<CampaignDto>> GetMyDonationsInCampaignRequestsAsync(string userId);


        Task<bool> UpdateProfileAsync(string userId, UpdateProfileDto dto);


    }
}
