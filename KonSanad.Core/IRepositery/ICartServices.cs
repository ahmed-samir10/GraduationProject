using KonSanad.Core.Dtos;

namespace KonSanad.Core.IRepositery
{
    public interface ICartService
    {
        Task<CartResponseDto> GetCartAsync(int userId);

        Task<string> AddItemAsync(int userId, AddToCartDto dto);

        Task AddItemsAsync(int userId, IEnumerable<AddToCartDto> dtos);

        Task<string> RemoveItemAsync(int userId, int productId);

        Task<string> RemoveItemsAsync(int userId, IEnumerable<int> productIds);

        Task<string> UpdateItemsQuantityAsync(int userId, IEnumerable<UpdateCartItemDto> items);

        Task<string> ClearCartAsync(int userId);
    }


}
