using TestFirstProject.DTOs.Payments;

namespace TestFirstProject.Services.Interfaces;

public interface IPaymentService
{
    Task<PaymentDto> InitiatePaymentAsync(InitiatePaymentRequest request, Guid userId);
    Task<PaymentDto> CapturePaymentAsync(Guid paymentId, Guid userId);
    Task<PaymentDto> RefundPaymentAsync(Guid paymentId, RefundRequest request, Guid userId);
    Task<WalletDto> GetWalletBalanceAsync(Guid userId);
    Task<WalletDto> TopUpWalletAsync(Guid userId, WalletTopUpRequest request);
}
