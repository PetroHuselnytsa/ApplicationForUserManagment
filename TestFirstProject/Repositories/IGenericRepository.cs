namespace TestFirstProject.Repositories
{
    /// <summary>
    /// Generic repository interface providing async CRUD operations
    /// and an IQueryable accessor for custom queries (filtering, pagination).
    /// </summary>
    public interface IGenericRepository<T> where T : class
    {
        /// <summary>Get an entity by its primary key.</summary>
        Task<T?> GetByIdAsync(Guid id);

        /// <summary>Get all entities as a read-only list.</summary>
        Task<IReadOnlyList<T>> GetAllAsync();

        /// <summary>Add a new entity.</summary>
        Task<T> AddAsync(T entity);

        /// <summary>Update an existing entity.</summary>
        Task UpdateAsync(T entity);

        /// <summary>Delete an entity.</summary>
        Task DeleteAsync(T entity);

        /// <summary>Expose IQueryable for custom queries (filtering, ordering, pagination).</summary>
        IQueryable<T> Query();
    }
}
