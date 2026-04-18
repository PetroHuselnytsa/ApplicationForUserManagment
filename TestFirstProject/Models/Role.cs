namespace TestFirstProject.Models
{
    /// <summary>
    /// Represents a role that can be assigned to users for authorization.
    /// </summary>
    public class Role
    {
        protected Role() { }

        public Role(string name)
        {
            Name = name;
        }

        public Guid Id { get; set; }
        public string Name { get; set; } = null!;

        // Navigation property
        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }
}
