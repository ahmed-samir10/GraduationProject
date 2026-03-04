using KonSanad.Core.Dtos;

namespace KonSanad.Core.IRepositery
{
    public interface ICartService
    {
        Task<CartResponseDto> GetCartAsync(string userId, string role);
        Task<string> AddToCartAsync(string userId, string role, object dto);
        Task<string> UpdateItemAsync(string userId, string role, int productId, int quantity);
        Task<string> RemoveItemAsync(string userId, string role, int productId);
        Task ClearCartAsync(string userId, string role);
        Task<SubmitResultDto> SubmitCartAsync(string userId, string role);
    }

}
