using KonSanad.Core.IRepositery;
using KonSanad.Repository.Data.Persistence;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KonSanad.Repository.Repository
{
    public class UnitOfWork : IUnitOfWork
    {

        private readonly KonSanadDbContext _context;
        private Hashtable _repostories;

        public UnitOfWork(KonSanadDbContext context)
        {
            _context = context;
        }

      
        public async Task<int> Complete()
            => await _context.SaveChangesAsync();


        // cratre a repo and return it 
        public IGenericRepository<T, Tkey> Repository<T, Tkey>() where T : BaseEntity<Tkey>
        {
            if (_repostories == null) _repostories = new Hashtable();

            var type = typeof(T).Name;

            if (!_repostories.ContainsKey(type))
            {
                var repository = new GenericRepository<T, Tkey>(_context);
                _repostories.Add(type, repository);
            }

            return (IGenericRepository<T, Tkey>)_repostories[type];
        }
    }
}
