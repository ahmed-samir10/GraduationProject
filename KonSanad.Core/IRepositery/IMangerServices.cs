using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KonSanad.Core.Dtos;
using KonSanad.Core.Dtos.ManagerDtos;
using KonSanad.Core.Entity.Enums;

namespace KonSanad.Core.IRepositery
{
    public interface IMangerServices
    {
        Task<string> MakeManagerAsync(int volunteerId);

        //----------------------------------------------------------------------------------
        // ✅ زودنا search و status و helpOrderId كـ optional parameters
        Task<object> GetHelpOrderAsync(int pageNumber, int pageSize, string? search, RequestedStatus? status, int? helpOrderId);
        //----------------------------------------------------------------------------------

        Task<IEnumerable<GetVolunteerDto>> GetVolunteerAsync(
         string? search = null,
         RequestedStatus? status = null,
         int pageNumber = 1,
         int pageSize = 8);
        Task<string> AssignVolunteerAsync(int HelpOderId, int VolunteerId, DateTime Start, DateTime End);
        Task<ViewReportDto> ViewReport(int HelpOrderId);
        Task <string> AcceptOrder(int orderId);
         Task<string> RejectOrder(int orderId);
        Task<IEnumerable<GetJoinVolunteerDto>> GetJoinVolunteer(
     int pageNumber,
     int pageSize,
     RequestedStatus? status); // ✅ UPDATED: إضافة parameter للفلترة
        Task<string> AcceptVolunteer(int orderId);
        Task<string> RejectVolunteer(int Id, string ReajectReason);

        Task<string> AcceptRequst(int Requsttype , int referenceId);
        Task<string> RejectRequst(int Requsttype, int referenceId, string Reason);

        // Get All Volunteer Requests [ Join a campaigns + Start Mission ] 
        // في IMangerServices
        Task<IEnumerable<GetVolunteerRequestsDto>> GetAllVolunteerRequestsAsync(
            int pageNumber,
            int pageSize,
            string? search = null,                      // سيرش في كل حقول الـ DTO
            RefType? requestType = null,                // فيلتر 1: حملة أو مهمة
            VolunteerMissionStatus? missionStatus = null); // فيلتر 2: حالة الطلب
        Task<VolunteerRequestDetailDto> GetRequestDetailsAsync(
    int referenceId, int type);

        Task<IEnumerable<GetAllDonationDto>> GetDonationAsync(
      int pageNumber,
      int pageSize,
      string? search = null,           // سيرش في حقول الـ DTO
      DonationCategory? category = null, // فيلتر بنوع التبرع
      RequestedStatus? status = null); // فيلتر بالحالة

        Task<GetAllDonationDetailsDto> ViewAllDetailsDonation(int DonationOrderId);
        



        Task<string> AcceptDonation(int DonationOrderId);

        Task<string> RejectDonation(int DonationOrderId, string RejectReason);


    
    }
}
