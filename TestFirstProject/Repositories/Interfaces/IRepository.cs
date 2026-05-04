namespace TestFirstProject.Repositories.Interfaces
{
    /// <summary>
    /// Marker interface for entities that support soft-delete.
    /// The generic repository checks this at runtime to set <see cref="IsDeleted"/>
    /// instead of removing the row from the database.
    /// </summary>
    public interface ISoftDeletable
    {
        bool IsDeleted { get; set; }
    }

    /// <summary>
    /// Generic repository contract for basic CRUD operations.
    /// All methods are async; soft-delete is applied automatically when
    /// the entity implements <see cref="ISoftDeletable"/>.
    /// </summary>
    /// <typeparam name="T">Entity type — must be a reference type.</typeparam>
    public interface IRepository<T> where T : class
    {
        /// <summary>Returns the entity with the given primary key, or null if not found.</summary>
        Task<T?> GetByIdAsync(Guid id);

        /// <summary>Returns all non-deleted entities matching the optional predicate.</summary>
        Task<IReadOnlyList<T>> GetAllAsync(
            System.Linq.Expressions.Expression<Func<T, bool>>? filter = null,
            int page = 1,
            int pageSize = 20);

        /// <summary>Returns the total count of non-deleted entities matching the optional predicate.</summary>
        Task<int> CountAsync(System.Linq.Expressions.Expression<Func<T, bool>>? filter = null);

        /// <summary>Adds the entity to the context (SaveChangesAsync must be called separately).</summary>
        Task AddAsync(T entity);

        /// <summary>Marks the entity as modified (SaveChangesAsync must be called separately).</summary>
        void Update(T entity);

        /// <summary>
        /// Removes the entity. If the entity implements <see cref="ISoftDeletable"/>,
        /// sets <c>IsDeleted = true</c> instead of deleting the row.
        /// </summary>
        void Delete(T entity);

        /// <summary>Persists all pending changes to the database.</summary>
        Task<int> SaveChangesAsync();
    }
}
