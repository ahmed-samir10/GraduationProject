using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KonSanad.Core.IRepositery
{
    public interface ICashServices
    {
        Task SetCashAsync(string Key, object Response, TimeSpan Exp);
        Task<string> GetCashAsync(string Key);

    }
}
