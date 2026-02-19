using KonSanad.Core.Dtos;

namespace KonSanad.Core.IRepositery
{
    public interface ICartService
    {
        Task<CartResponseDto> GetCartAsync(int userId);

        Task<string> AddItemAsync(int userId, AddToCartDto dto);

     

        Task<string> RemoveItemAsync(int userId, int productId);



        Task<string> UpdateItemQuantityAsync(int userId, int productId, int quantity);

        Task<string> ClearCartAsync(int userId);

        Task<SubmitResultDto> SubmitCartAsync(int userId, IEnumerable<AddToCartDto>? items = null);
    }


}
