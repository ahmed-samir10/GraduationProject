using KonSanad.Core.IRepositery;
using KonSanad.Repository.Data.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace KonSanad.Repository.Repository
{
    public class GenericRepository<T, Tkey> : IGenericRepository<T, Tkey> where T : BaseEntity<Tkey>
    {
        private readonly KonSanadDbContext _context;
        public GenericRepository(KonSanadDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<T>> GetAllAsync()
        {
            return await _context.Set<T>().ToListAsync();
        }

        public async Task<T> FirstOrDefaultAsync(
       Expression<Func<T, bool>> predicate,
       Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null)
        {
            IQueryable<T> query = _context.Set<T>();

            if (include != null)
                query = include(query);

            return await query.FirstOrDefaultAsync(predicate);
        }
        public async Task<T> FirstAsync(Expression<Func<T, bool>> predicate)
        {
            return await _context.Set<T>().FirstAsync(predicate);
        }

        public async Task<IEnumerable<T>> FindAllAsync(
     Expression<Func<T, bool>> predicate,
     Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null)
        {
            IQueryable<T> query = _context.Set<T>().Where(predicate);

            if (include != null)
                query = include(query);

            return await query.ToListAsync();
        }

        public async Task AddAsync(T entity)
           => await _context.Set<T>().AddAsync(entity);
        public void Update(T entity)
            => _context.Set<T>().Update(entity);
        public void Delete(T entity)
            => _context.Set<T>().Remove(entity);

        public async Task<T> GetByIdAsync(int id)
       => await _context.Set<T>().FindAsync(id);

        public async Task<T> GetUserIdAsync(string id)
       => await _context.Set<T>().FindAsync(id);


    }
}
