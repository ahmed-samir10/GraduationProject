using KonSanad.Core.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KonSanad.Core.IRepositery
{
    public interface IMangerServices
    {
            Task<IEnumerable<GetHelpOrderDto>> GetHelpOrderAsync();
        Task<IEnumerable<GetVolunteerDto>> GetVolunteerAsync();
        Task<string> AssignVolunteerAsync(int HelpOderId, int VolunteerId);
        Task<ViewReportDto> ViewReport(int HelpOrderId);
        Task <string> AcceptOrder(int orderId);
         Task<string> RejectOrder(int orderId);


    }
}
