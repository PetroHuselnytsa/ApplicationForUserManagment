using TestFirstProject.DTOs;

namespace TestFirstProject.Services.Interfaces
{
    public interface IPaymentService
    {
        Task<PaymentResponse> InitiatePaymentAsync(InitiatePaymentRequest request, Guid userId);
        Task<PaymentResponse> CapturePaymentAsync(Guid paymentId, Guid userId);
        Task<PaymentResponse> RefundPaymentAsync(Guid paymentId, RefundPaymentRequest request, Guid userId);
        Task<WalletResponse> GetWalletBalanceAsync(Guid userId);
        Task<WalletTopUpResponse> TopUpWalletAsync(Guid userId, TopUpWalletRequest request);
    }
}
