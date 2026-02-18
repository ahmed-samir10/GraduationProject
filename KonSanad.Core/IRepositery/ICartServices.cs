using KonSanad.Core.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KonSanad.Core.IRepositery
{
    public interface ICartService
    {
        Task<CartResponseDto> GetCartAsync(int userId);

        Task AddItemAsync(int userId, AddToCartDto dto);

        Task<string> RemoveItemAsync(int userId, int productId);

        Task<string> ClearCartAsync(int userId);
    }


}
