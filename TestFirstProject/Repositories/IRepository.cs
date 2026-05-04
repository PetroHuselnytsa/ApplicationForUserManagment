using System.Linq.Expressions;

namespace TestFirstProject.Repositories
{
    /// <summary>
    /// Generic async repository contract for basic CRUD operations.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    public interface IRepository<T> where T : class
    {
        /// <summary>Returns the entity with the given primary key, or null if not found.</summary>
        Task<T?> GetByIdAsync(Guid id);

        /// <summary>
        /// Returns all entities that satisfy <paramref name="predicate"/>.
        /// Pass <c>null</c> to return all rows.
        /// </summary>
        Task<IReadOnlyList<T>> GetAllAsync(Expression<Func<T, bool>>? predicate = null);

        /// <summary>Returns a queryable that callers can further compose (e.g. for pagination).</summary>
        IQueryable<T> Query(Expression<Func<T, bool>>? predicate = null);

        /// <summary>Inserts a new entity and saves immediately.</summary>
        Task AddAsync(T entity);

        /// <summary>Marks an entity as modified; call <see cref="SaveChangesAsync"/> to persist.</summary>
        void Update(T entity);

        /// <summary>Persists all pending changes to the database.</summary>
        Task<int> SaveChangesAsync();
    }
}
