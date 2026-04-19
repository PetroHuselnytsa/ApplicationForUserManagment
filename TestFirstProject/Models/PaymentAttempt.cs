using TestFirstProject.Models.Enums;

namespace TestFirstProject.Models
{
    /// <summary>
    /// Tracks individual payment attempts and their outcomes.
    /// </summary>
    public class PaymentAttempt
    {
        public Guid Id { get; set; }
        public Guid PaymentId { get; set; }
        public Payment Payment { get; set; } = null!;

        public PaymentStatus Status { get; set; }
        public string? FailureReason { get; set; }
        public string? GatewayResponse { get; set; }
        public DateTime AttemptedAt { get; set; } = DateTime.UtcNow;
    }
}
