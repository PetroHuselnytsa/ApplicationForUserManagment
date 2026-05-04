using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using TestFirstProject.Contexts;

namespace TestFirstProject.Repositories
{
    /// <summary>
    /// EF Core implementation of <see cref="IRepository{T}"/> backed by <see cref="PersonsContext"/>.
    /// All database operations are async.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly PersonsContext _context;
        private readonly DbSet<T> _set;

        public Repository(PersonsContext context)
        {
            _context = context;
            _set = context.Set<T>();
        }

        /// <inheritdoc/>
        public async Task<T?> GetByIdAsync(Guid id)
            => await _set.FindAsync(id);

        /// <inheritdoc/>
        public async Task<IReadOnlyList<T>> GetAllAsync(Expression<Func<T, bool>>? predicate = null)
            => await BuildQuery(predicate).ToListAsync();

        /// <inheritdoc/>
        public IQueryable<T> Query(Expression<Func<T, bool>>? predicate = null)
            => BuildQuery(predicate);

        private IQueryable<T> BuildQuery(Expression<Func<T, bool>>? predicate)
        {
            IQueryable<T> query = _set.AsNoTracking();
            if (predicate != null)
                query = query.Where(predicate);
            return query;
        }

        /// <inheritdoc/>
        public async Task AddAsync(T entity)
        {
            await _set.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public void Update(T entity)
            => _set.Update(entity);

        /// <inheritdoc/>
        public async Task<int> SaveChangesAsync()
            => await _context.SaveChangesAsync();
    }
}
