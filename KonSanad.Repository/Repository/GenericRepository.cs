using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using KonSanad.Core.IRepositery;
using KonSanad.Repository.Data.Persistence;
using Microsoft.EntityFrameworkCore;

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
       


        public async Task AddAsync(T entity)
           => await _context.Set<T>().AddAsync(entity);
        public void Update(T entity)
            => _context.Set<T>().Update(entity);
        public void Delete(T entity)
            => _context.Set<T>().Remove(entity);

        public async Task<T> GetByIdAsync(int id)
       => await _context.Set<T>().FindAsync(id);

    }
}
