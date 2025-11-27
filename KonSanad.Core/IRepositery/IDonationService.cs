using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KonSanad.Core.Dtos;

namespace KonSanad.Core.IRepositery
{
    public interface IDonationService
    {
        Task<DonationResponseDto> CreateDonationAsync(CreateDonationRequestDto request, int donorId);
       // Task<IEnumerable<DonationResponseDto>> GetDonorDonationsAsync(KonSanad.Core.Dtos.DonorRequetDto v);
        Task<IEnumerable<object>> GetDonorDonationsAsync(int donorId);

        Task<IEnumerable<object>> GetBeneficiaryDonationsAsync(int beneficiaryId);

       // Task<IEnumerable<DonorDonatedItemDto>> GetDonorItemsToBeneficiariesAsync(int donorId);

        // New: return all donation operations (orders) across the system
        Task<IEnumerable<object>> GetAllDonationsAsync();
    }
}
