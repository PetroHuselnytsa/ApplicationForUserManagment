namespace TestFirstProject.Models
{
    public class User
    {
        protected User() { }

        public User(string email, string passwordHash, string name)
        {
            Email = email;
            PasswordHash = passwordHash;
            Name = name;
        }

        public Guid Id { get; set; }
        public string Email { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public string Name { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation property
        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    }
}
