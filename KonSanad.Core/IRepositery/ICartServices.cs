using KonSanad.Core.Dtos;

namespace KonSanad.Core.IRepositery
{
    public interface ICartService
    {
        Task<CartResponseDto> GetCartAsync(int userId, string role);
        Task<string> AddToCartAsync(int userId, string role, object dto);
        Task<string> UpdateItemAsync(int userId, string role, int productId, int quantity);
        Task<string> UpdateAmountCash(int userId, string role, int cashDonationId, int Amount);
        Task<string> RemoveItemAsync(int userId, string role, int productId);
        Task<string> RemoveCashAsync(int userId, string role, int cashDonationId);
        Task ClearCartAsync(int userId, string role);
        Task<SubmitResultDto> SubmitCartAsync(int userId, string role);
    }

}
