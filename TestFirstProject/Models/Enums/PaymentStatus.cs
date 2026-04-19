namespace TestFirstProject.Models.Enums;

/// <summary>
/// Payment processing states.
/// </summary>
public enum PaymentStatus
{
    Pending = 0,
    Authorized = 1,
    Captured = 2,
    Failed = 3,
    Refunded = 4
}
