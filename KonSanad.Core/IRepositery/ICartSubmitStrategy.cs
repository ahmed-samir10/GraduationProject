using KonSanad.Core.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KonSanad.Core.IRepositery
{
    public interface ICartSubmitStrategy
    {
        Task<SubmitResultDto> SubmitAsync(string userId, CartResponseDto cart);
    }
}
