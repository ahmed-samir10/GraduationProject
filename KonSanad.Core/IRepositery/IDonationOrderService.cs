using KonSanad.Core.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KonSanad.Core.IRepositery
{
    public interface IDonationOrderService
    {
        Task<DonationResponseDto> CreateDonationAsync(CreateDonationRequestDto request, int donorId);
        Task<IEnumerable<object>> GetDonorDonationsAsync(int donorId);
    }
}
