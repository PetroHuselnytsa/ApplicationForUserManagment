using TestFirstProject.Models.Enums;
using TestFirstProject.Services.Interfaces;

namespace TestFirstProject.Services.Implementations
{
    /// <summary>
    /// Base class for mock payment gateways that always succeed.
    /// Production implementations would call real payment APIs.
    /// </summary>
    public abstract class MockPaymentGateway : IPaymentGateway
    {
        public abstract PaymentMethod SupportedMethod { get; }

        public Task<(bool Success, string TransactionId, string? FailureReason)> AuthorizeAsync(decimal amount)
            => Task.FromResult((true, Guid.NewGuid().ToString(), (string?)null));

        public Task<(bool Success, string? FailureReason)> CaptureAsync(string transactionId, decimal amount)
            => Task.FromResult((true, (string?)null));

        public Task<(bool Success, string? FailureReason)> RefundAsync(string transactionId, decimal amount)
            => Task.FromResult((true, (string?)null));
    }

    public class CreditCardGateway : MockPaymentGateway
    {
        public override PaymentMethod SupportedMethod => PaymentMethod.CreditCard;
    }

    public class PayPalGateway : MockPaymentGateway
    {
        public override PaymentMethod SupportedMethod => PaymentMethod.PayPal;
    }

    public class BankTransferGateway : MockPaymentGateway
    {
        public override PaymentMethod SupportedMethod => PaymentMethod.BankTransfer;
    }

    public class WalletGateway : MockPaymentGateway
    {
        public override PaymentMethod SupportedMethod => PaymentMethod.Wallet;
    }
}
