namespace TestFirstProject.DTOs
{
    // --- Payments ---

    public record InitiatePaymentRequest(
        Guid OrderId,
        string PaymentMethod);

    public record CapturePaymentRequest();

    public record RefundPaymentRequest(
        decimal? Amount,
        string? Reason);

    public record PaymentResponse(
        Guid Id,
        Guid OrderId,
        string Method,
        string Status,
        decimal Amount,
        decimal RefundedAmount,
        string? TransactionId,
        DateTime CreatedAt);

    public record WalletResponse(
        decimal Balance,
        DateTime UpdatedAt);

    public record TopUpWalletRequest(
        decimal Amount);

    public record WalletTopUpResponse(
        decimal NewBalance,
        decimal AmountAdded);
}
