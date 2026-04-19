using TestFirstProject.Models.Enums;

namespace TestFirstProject.DTOs.Payments;

// --- Request DTOs ---

public record InitiatePaymentRequest(
    Guid OrderId,
    string PaymentMethod  // CreditCard, PayPal, BankTransfer, Wallet
);

public record RefundRequest(
    decimal? Amount  // null = full refund
);

public record WalletTopUpRequest(
    decimal Amount
);

// --- Response DTOs ---

public record PaymentDto(
    Guid Id,
    Guid OrderId,
    PaymentMethod Method,
    PaymentStatus Status,
    decimal Amount,
    decimal RefundedAmount,
    string? TransactionReference,
    DateTime CreatedAt,
    DateTime? CapturedAt,
    List<PaymentAttemptDto> Attempts
);

public record PaymentAttemptDto(
    Guid Id,
    bool IsSuccessful,
    string? FailureReason,
    DateTime AttemptedAt
);

public record WalletDto(
    decimal Balance,
    DateTime UpdatedAt
);
