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

        Task<IEnumerable<OrderItemDto>> GetAllDonationItemsAsync(int pagenum, int pagesize);
        Task<OrderItemDto?> GetOrderItemByIdAsync(int id);

    }
}