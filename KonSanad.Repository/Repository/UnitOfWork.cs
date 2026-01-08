using KonSanad.Core.IRepositery;
using KonSanad.Repository.Data.Persistence;
using System.Collections;

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
//namespace KonSanad.Repository.Repository
//{
//    public class UnitOfWork : IUnitOfWork
//    {

//        private readonly KonSanadDbContext _context;
//        private ConcurrentDictionary<string, object> _repostories;

//        public UnitOfWork(KonSanadDbContext context)
//        => (_context, _repostories) = (context, new());

//        public async Task<int> Complete() => await _context.SaveChangesAsync();


//        // cratre a repo and return it 
//        public IGenericRepository<T, Tkey> Repository<T, Tkey>() where T : BaseEntity<Tkey>
//           => (IGenericRepository<T, Tkey>)  // Cast
//             _repostories.GetOrAdd(typeof(T).Name, _ => new GenericRepository<T, Tkey>(_context));

//    }
//}
