using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using TestFirstProject.Contexts;
using TestFirstProject.Repositories.Interfaces;

namespace TestFirstProject.Repositories.Implementations
{
    /// <summary>
    /// Generic EF Core repository backed by <see cref="PersonsContext"/>.
    /// Provides async CRUD with built-in soft-delete support: if the entity
    /// implements <see cref="ISoftDeletable"/>, <see cref="Delete"/> sets
    /// <c>IsDeleted = true</c> instead of removing the row.
    /// </summary>
    /// <typeparam name="T">Entity type — must be a reference type tracked by EF.</typeparam>
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly PersonsContext _context;
        private readonly DbSet<T> _set;

        public Repository(PersonsContext context)
        {
            _context = context;
            _set = context.Set<T>();
        }

        /// <inheritdoc />
        public async Task<T?> GetByIdAsync(Guid id)
        {
            return await _set.FindAsync(id);
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<T>> GetAllAsync(
            Expression<Func<T, bool>>? filter = null,
            int page = 1,
            int pageSize = 20)
        {
            var query = _set.AsQueryable();

            if (filter is not null)
                query = query.Where(filter);

            return await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        /// <inheritdoc />
        public async Task<int> CountAsync(Expression<Func<T, bool>>? filter = null)
        {
            var query = _set.AsQueryable();

            if (filter is not null)
                query = query.Where(filter);

            return await query.CountAsync();
        }

        /// <inheritdoc />
        public async Task AddAsync(T entity)
        {
            await _set.AddAsync(entity);
        }

        /// <inheritdoc />
        public void Update(T entity)
        {
            _set.Update(entity);
        }

        /// <inheritdoc />
        public void Delete(T entity)
        {
            if (entity is ISoftDeletable softDeletable)
            {
                // Soft-delete: mark the flag and let EF update the row
                softDeletable.IsDeleted = true;
                _set.Update(entity);
            }
            else
            {
                // Hard-delete for entities that don't implement ISoftDeletable
                _set.Remove(entity);
            }
        }

        /// <inheritdoc />
        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}
