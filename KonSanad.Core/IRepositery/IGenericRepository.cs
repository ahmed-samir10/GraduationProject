using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KonSanad.Core.IRepositery
{
    public interface IGenericRepository<T, Tkey> where T : BaseEntity<Tkey>
    {

        Task<IReadOnlyList<T>> GetAllAsync();
        Task<T> GetByIdAsync(int id);
        Task AddAsync(T entity);
        void Update(T entity);
        void Delete(T entity);
    }
}
