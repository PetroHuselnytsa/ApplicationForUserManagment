namespace TestFirstProject.Repositories
{
    /// <summary>
    /// Generic repository abstraction providing common async CRUD operations.
    /// </summary>
    /// <typeparam name="T">The entity type managed by this repository.</typeparam>
    public interface IRepository<T> where T : class
    {
        /// <summary>Returns a single entity by its primary key, or null when not found.</summary>
        Task<T?> GetByIdAsync(Guid id);

        /// <summary>Returns an <see cref="IQueryable{T}"/> over the entity set for further composition.</summary>
        IQueryable<T> GetAll();

        /// <summary>Adds a new entity to the context and persists it.</summary>
        Task<T> AddAsync(T entity);

        /// <summary>Marks an entity as modified and persists changes.</summary>
        Task UpdateAsync(T entity);

        /// <summary>Removes an entity from the context and persists the deletion (hard delete).</summary>
        Task DeleteAsync(T entity);

        /// <summary>Persists all pending changes to the database.</summary>
        Task SaveChangesAsync();
    }
}
