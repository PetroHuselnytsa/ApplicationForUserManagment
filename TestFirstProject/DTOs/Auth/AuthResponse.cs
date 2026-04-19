namespace TestFirstProject.DTOs.Auth
{
    public class AuthResponse
    {
        public string Token { get; set; } = null!;
        public Guid UserId { get; set; }
        public string Username { get; set; } = null!;
        public string Role { get; set; } = null!;
        public DateTime ExpiresAt { get; set; }
    }
}
