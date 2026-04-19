using Microsoft.EntityFrameworkCore;
using TestFirstProject.Contexts;
using TestFirstProject.DTOs.Payments;
using TestFirstProject.Exceptions;
using TestFirstProject.Models;
using TestFirstProject.Models.Enums;
using TestFirstProject.Services.Interfaces;

namespace TestFirstProject.Services.Implementations;

public class PaymentService : IPaymentService
{
    private readonly PersonsContext _context;

    public PaymentService(PersonsContext context)
    {
        _context = context;
    }

    public async Task<PaymentDto> InitiatePaymentAsync(InitiatePaymentRequest request, Guid userId)
    {
        if (!Enum.TryParse<PaymentMethod>(request.PaymentMethod, true, out var method))
            throw new ValidationException($"Invalid payment method: '{request.PaymentMethod}'. Valid: CreditCard, PayPal, BankTransfer, Wallet");

        var order = await _context.Orders
            .FirstOrDefaultAsync(o => o.Id == request.OrderId && o.UserId == userId)
            ?? throw new NotFoundException($"Order '{request.OrderId}' not found.");

        if (order.Status != OrderStatus.Pending)
            throw new ValidationException($"Order must be in Pending status to initiate payment. Current: {order.Status}");

        // Check if there's already a successful payment
        var existingPayment = await _context.Payments
            .AnyAsync(p => p.OrderId == order.Id && (p.Status == PaymentStatus.Captured || p.Status == PaymentStatus.Authorized));

        if (existingPayment)
            throw new ConflictException("Order already has an active payment.");

        var now = DateTime.UtcNow;
        var payment = new Payment
        {
            OrderId = order.Id,
            Method = method,
            Amount = order.TotalAmount,
            CreatedAt = now
        };

        _context.Payments.Add(payment);

        // Simulate payment authorization
        var attempt = new PaymentAttempt
        {
            PaymentId = payment.Id,
            AttemptedAt = now
        };

        if (method == PaymentMethod.Wallet)
        {
            // Wallet payment: check balance
            var wallet = await _context.Wallets.FirstOrDefaultAsync(w => w.UserId == userId);
            if (wallet == null || wallet.Balance < order.TotalAmount)
            {
                attempt.IsSuccessful = false;
                attempt.FailureReason = "Insufficient wallet balance";
                payment.Status = PaymentStatus.Failed;
            }
            else
            {
                // Deduct from wallet immediately for wallet payments
                wallet.Balance -= order.TotalAmount;
                wallet.UpdatedAt = now;

                attempt.IsSuccessful = true;
                attempt.GatewayResponse = "Wallet payment authorized and captured";
                payment.Status = PaymentStatus.Captured;
                payment.CapturedAt = now;
                payment.TransactionReference = $"WALLET-{Guid.NewGuid():N}".Substring(0, 20);

                // Advance order to Confirmed
                OrderStateMachine.ValidateTransition(order.Status, OrderStatus.Confirmed);
                order.Status = OrderStatus.Confirmed;
                order.UpdatedAt = now;

                _context.OrderStatusHistories.Add(new OrderStatusHistory
                {
                    OrderId = order.Id,
                    FromStatus = OrderStatus.Pending,
                    ToStatus = OrderStatus.Confirmed,
                    ChangedByUserId = userId,
                    Notes = "Payment captured via wallet",
                    ChangedAt = now
                });
            }
        }
        else
        {
            // Simulate successful authorization for other methods
            attempt.IsSuccessful = true;
            attempt.GatewayResponse = $"Authorized via {method}";
            payment.Status = PaymentStatus.Authorized;
            payment.TransactionReference = $"{method.ToString().ToUpper()}-{Guid.NewGuid():N}".Substring(0, 24);
        }

        _context.PaymentAttempts.Add(attempt);
        await _context.SaveChangesAsync();

        return await MapToPaymentDto(payment.Id);
    }

    public async Task<PaymentDto> CapturePaymentAsync(Guid paymentId, Guid userId)
    {
        var payment = await _context.Payments
            .Include(p => p.Order)
            .FirstOrDefaultAsync(p => p.Id == paymentId)
            ?? throw new NotFoundException($"Payment '{paymentId}' not found.");

        if (payment.Status != PaymentStatus.Authorized)
            throw new ValidationException($"Payment must be Authorized to capture. Current: {payment.Status}");

        var now = DateTime.UtcNow;
        payment.Status = PaymentStatus.Captured;
        payment.CapturedAt = now;

        // Record capture attempt
        _context.PaymentAttempts.Add(new PaymentAttempt
        {
            PaymentId = payment.Id,
            IsSuccessful = true,
            GatewayResponse = "Payment captured successfully",
            AttemptedAt = now
        });

        // Advance order to Confirmed
        var order = payment.Order;
        if (order.Status == OrderStatus.Pending)
        {
            OrderStateMachine.ValidateTransition(order.Status, OrderStatus.Confirmed);
            order.Status = OrderStatus.Confirmed;
            order.UpdatedAt = now;

            _context.OrderStatusHistories.Add(new OrderStatusHistory
            {
                OrderId = order.Id,
                FromStatus = OrderStatus.Pending,
                ToStatus = OrderStatus.Confirmed,
                ChangedByUserId = userId,
                Notes = "Payment captured",
                ChangedAt = now
            });
        }

        await _context.SaveChangesAsync();
        return await MapToPaymentDto(payment.Id);
    }

    public async Task<PaymentDto> RefundPaymentAsync(Guid paymentId, RefundRequest request, Guid userId)
    {
        var payment = await _context.Payments
            .Include(p => p.Order)
            .FirstOrDefaultAsync(p => p.Id == paymentId)
            ?? throw new NotFoundException($"Payment '{paymentId}' not found.");

        if (payment.Status != PaymentStatus.Captured)
            throw new ValidationException($"Payment must be Captured to refund. Current: {payment.Status}");

        var refundAmount = request.Amount ?? payment.Amount - payment.RefundedAmount; // Full refund if no amount specified
        if (refundAmount <= 0)
            throw new ValidationException("Refund amount must be greater than zero.");

        if (refundAmount > payment.Amount - payment.RefundedAmount)
            throw new ValidationException($"Refund amount ({refundAmount:C}) exceeds refundable amount ({payment.Amount - payment.RefundedAmount:C}).");

        var now = DateTime.UtcNow;
        payment.RefundedAmount += refundAmount;

        // If fully refunded, mark as Refunded
        if (payment.RefundedAmount >= payment.Amount)
        {
            payment.Status = PaymentStatus.Refunded;
        }

        _context.PaymentAttempts.Add(new PaymentAttempt
        {
            PaymentId = payment.Id,
            IsSuccessful = true,
            GatewayResponse = $"Refund of {refundAmount:C} processed successfully",
            AttemptedAt = now
        });

        // Refund to wallet if payment was via wallet
        if (payment.Method == PaymentMethod.Wallet)
        {
            var wallet = await _context.Wallets.FirstOrDefaultAsync(w => w.UserId == payment.Order.UserId);
            if (wallet != null)
            {
                wallet.Balance += refundAmount;
                wallet.UpdatedAt = now;
            }
        }

        await _context.SaveChangesAsync();
        return await MapToPaymentDto(payment.Id);
    }

    public async Task<WalletDto> GetWalletBalanceAsync(Guid userId)
    {
        var wallet = await GetOrCreateWalletAsync(userId);
        return new WalletDto(wallet.Balance, wallet.UpdatedAt);
    }

    public async Task<WalletDto> TopUpWalletAsync(Guid userId, WalletTopUpRequest request)
    {
        if (request.Amount <= 0)
            throw new ValidationException("Top-up amount must be greater than zero.");

        var wallet = await GetOrCreateWalletAsync(userId);
        wallet.Balance += request.Amount;
        wallet.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return new WalletDto(wallet.Balance, wallet.UpdatedAt);
    }

    // --- Private helpers ---

    private async Task<Wallet> GetOrCreateWalletAsync(Guid userId)
    {
        var wallet = await _context.Wallets.FirstOrDefaultAsync(w => w.UserId == userId);
        if (wallet == null)
        {
            wallet = new Wallet
            {
                UserId = userId,
                Balance = 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.Wallets.Add(wallet);
            await _context.SaveChangesAsync();
        }
        return wallet;
    }

    private async Task<PaymentDto> MapToPaymentDto(Guid paymentId)
    {
        var payment = await _context.Payments
            .Include(p => p.Attempts)
            .FirstAsync(p => p.Id == paymentId);

        var attempts = payment.Attempts
            .OrderBy(a => a.AttemptedAt)
            .Select(a => new PaymentAttemptDto(a.Id, a.IsSuccessful, a.FailureReason, a.AttemptedAt))
            .ToList();

        return new PaymentDto(
            payment.Id, payment.OrderId, payment.Method, payment.Status,
            payment.Amount, payment.RefundedAmount, payment.TransactionReference,
            payment.CreatedAt, payment.CapturedAt, attempts
        );
    }
}
