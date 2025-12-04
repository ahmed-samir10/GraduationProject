using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace KonSanad.Core.IRepositery
{
    public interface IGenericRepository<T, Tkey> where T : BaseEntity<Tkey>
    {

        Task<IReadOnlyList<T>> GetAllAsync();
        Task<T> GetByIdAsync(int id);
       
       Task<T> GetUserIdAsync(string id);
       
        Task AddAsync(T entity);
        void Update(T entity);
        void Delete(T entity);
         Task<T> FirstOrDefaultAsync(
     Expression<Func<T, bool>> predicate,
     Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null);
        Task<IEnumerable<T>> FindAllAsync(
              Expression<Func<T, bool>> predicate,
              Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null);
    }
}
