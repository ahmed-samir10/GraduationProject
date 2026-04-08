using KonSanad.Core.Dtos;
using KonSanad.Core.Entity.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KonSanad.Core.IRepositery
{
    public interface IMangerServices
    {
        Task<string> MakeManagerAsync(int volunteerId);
            Task<IEnumerable<GetHelpOrderDto>> GetHelpOrderAsync(int pageNumber, int pageSize);
        Task<IEnumerable<GetVolunteerDto>> GetVolunteerAsync();
        Task<string> AssignVolunteerAsync(int HelpOderId, int VolunteerId);
        Task<ViewReportDto> ViewReport(int HelpOrderId);
        Task <string> AcceptOrder(int orderId);
         Task<string> RejectOrder(int orderId);
        Task<IEnumerable<GetJoinVolunteerDto>> GetJoinVolunteer(int pageNumber, int pageSize);
        Task<string> AcceptVolunteer(int orderId);
        Task<string> RejectVolunteer(int Id, string ReajectReason);

        Task<string> AcceptRequst(int Requsttype , int referenceId);
        Task<string> RejectRequst(int Requsttype, int referenceId, string Reason);
     
        // Get All Volunteer Requests [ Join a campaigns + Start Mission ] 
        Task<IEnumerable<GetVolunteerRequestsDto>> GetAllVolunteerRequestsAsync(int pageNumber, int pageSize);
        Task<VolunteerRequestDetailDto> GetRequestDetailsAsync(
    int referenceId, int type);

        Task<IEnumerable<GetAllDonationDto>> GetDonationAsync(int pageNumber, int pageSize);

        Task<GetAllDonationDetailsDto> ViewAllDetailsDonation(int DonationOrderId);
        Task<VolunteerRequestDetailDto> GetRequestDetailsAsync(
  int referenceId, int type);

        Task<string> AcceptDonation(int DonationOrderId);

        Task<string> RejectDonation(int DonationOrderId, string RejectReason);

    }
}
