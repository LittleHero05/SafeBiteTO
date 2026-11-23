using Microsoft.EntityFrameworkCore;
using SafeBite.Api.Data;

namespace SafeBite.Api.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly SafeBiteDbContext _context;
        protected readonly DbSet<T> _set;

        public Repository(SafeBiteDbContext context)
        {
            _context = context;
            _set = context.Set<T>();
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync() =>
            await _set.AsNoTracking().ToListAsync();

        public virtual async Task<T?> GetByIdAsync(int id) =>
            await _set.FindAsync(id);

        public async Task AddAsync(T entity) => await _set.AddAsync(entity);

        public void Update(T entity) => _set.Update(entity);

        public void Delete(T entity) => _set.Remove(entity);

        public async Task SaveAsync() => await _context.SaveChangesAsync();
    }
}