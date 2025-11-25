using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KonSanad.Core.IRepositery
{
   public interface IUnitOfWork
    {

        IGenericRepository<T, Tkey> Repository<T, Tkey>() where T : BaseEntity<Tkey>;

        Task<int> Complete();
    }
}
