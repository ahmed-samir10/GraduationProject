using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace KonSanad.Core.IRepositery
{
    public interface IUnitOfWork
    {

        IGenericRepository<T, Tkey> Repository<T, Tkey>() where T : BaseEntity<Tkey>;

        Task<int> Complete();
       

    }
}