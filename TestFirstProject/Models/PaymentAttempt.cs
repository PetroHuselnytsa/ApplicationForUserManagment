namespace TestFirstProject.Models;

/// <summary>
/// Individual payment attempt with failure reason tracking.
/// </summary>
public class PaymentAttempt
{
    public Guid Id { get; set; }
    public Guid PaymentId { get; set; }
    public Payment Payment { get; set; } = null!;

    public bool IsSuccessful { get; set; }
    public string? FailureReason { get; set; }
    public string? GatewayResponse { get; set; }
    public DateTime AttemptedAt { get; set; }
}
