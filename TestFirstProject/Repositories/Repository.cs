using Microsoft.EntityFrameworkCore;
using TestFirstProject.Contexts;

namespace TestFirstProject.Repositories
{
    /// <summary>
    /// Generic EF Core repository backed by <see cref="PersonsContext"/>.
    /// All operations are asynchronous.
    /// </summary>
    /// <typeparam name="T">The entity type managed by this repository.</typeparam>
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly PersonsContext _context;
        private readonly DbSet<T> _dbSet;

        public Repository(PersonsContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        /// <inheritdoc/>
        public async Task<T?> GetByIdAsync(Guid id)
        {
            return await _dbSet.FindAsync(id);
        }

        /// <inheritdoc/>
        public IQueryable<T> GetAll()
        {
            return _dbSet.AsNoTracking();
        }

        /// <inheritdoc/>
        public async Task<T> AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        /// <inheritdoc/>
        public async Task UpdateAsync(T entity)
        {
            _dbSet.Update(entity);
            await _context.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task DeleteAsync(T entity)
        {
            _dbSet.Remove(entity);
            await _context.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
