using KonSanad.Core.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KonSanad.Core.IRepositery
{
    public interface IDonationOrderService
    {
        // generic creation (kept for compatibility) — beneficiaryId optional
        Task<DonationResponseDto> CreateDonationAsync(CreateDonationRequestDto request, int donorId);

        // explicit endpoints for front-end: donate to campaign
        Task<DonationResponseDto> CreateDonationToCampaignAsync(CreateDonationRequestDto request, int donorId, int campaignId);

        // explicit endpoints for front-end: donate to a beneficiary (direct handover)
        Task<DonationResponseDto> CreateDonationToBeneficiaryAsync(CreateDonationRequestDto request, int donorId, int beneficiaryId);

        Task<IEnumerable<object>> GetDonorDonationsAsync();
    }
}