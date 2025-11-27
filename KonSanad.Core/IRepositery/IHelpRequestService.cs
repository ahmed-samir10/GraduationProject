using KonSanad.Core.Dtos;
using KonSanad.Core.IRepositery;


using KonSanad.Core.Dtos;
using System.Threading.Tasks;

namespace KonSanad.Core.IRepositery
{
    public interface IHelpRequestService
    {
        Task<HelpRequestResponseDto> RequestExistingProductAsync(int beneficiaryId, ExistingProductRequestDto request);
        Task<HelpRequestResponseDto> RequestNewProductAsync(int beneficiaryId, NewProductRequestDto request);
    }
}