namespace TestFirstProject.Models
{
    /// <summary>
    /// Represents an authorization role (e.g., Admin, User, PremiumUser).
    /// </summary>
    public class Role
    {
        public Guid Id { get; set; }

        /// <summary>
        /// Unique role name used in authorization policies.
        /// </summary>
        public string Name { get; set; } = null!;

        // Navigation property
        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }
}
