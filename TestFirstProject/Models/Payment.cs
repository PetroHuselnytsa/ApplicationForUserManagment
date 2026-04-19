using TestFirstProject.Models.Enums;

namespace TestFirstProject.Models
{
    /// <summary>
    /// Payment associated with an order.
    /// </summary>
    public class Payment
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public Order Order { get; set; } = null!;

        public PaymentMethod Method { get; set; }
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

        public decimal Amount { get; set; }
        public decimal RefundedAmount { get; set; }

        public string? TransactionId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<PaymentAttempt> Attempts { get; set; } = new List<PaymentAttempt>();
    }
}
