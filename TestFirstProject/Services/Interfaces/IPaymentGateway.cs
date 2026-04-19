using TestFirstProject.Models.Enums;

namespace TestFirstProject.Services.Interfaces
{
    /// <summary>
    /// Swappable payment gateway interface. Implementations are mock/in-memory.
    /// </summary>
    public interface IPaymentGateway
    {
        PaymentMethod SupportedMethod { get; }
        Task<(bool Success, string TransactionId, string? FailureReason)> AuthorizeAsync(decimal amount);
        Task<(bool Success, string? FailureReason)> CaptureAsync(string transactionId, decimal amount);
        Task<(bool Success, string? FailureReason)> RefundAsync(string transactionId, decimal amount);
    }
}
