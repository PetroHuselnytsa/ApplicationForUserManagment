using TestFirstProject.Models.Enums;

namespace TestFirstProject.Models;

/// <summary>
/// Payment record for an order. Supports multiple payment methods
/// and partial refunds.
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

    /// <summary>
    /// External transaction reference (e.g., PayPal transaction ID).
    /// </summary>
    public string? TransactionReference { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? CapturedAt { get; set; }

    // Navigation
    public ICollection<PaymentAttempt> Attempts { get; set; } = new List<PaymentAttempt>();
}
