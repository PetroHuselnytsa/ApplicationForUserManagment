using Microsoft.EntityFrameworkCore;
using TestFirstProject.Contexts;
using TestFirstProject.DTOs;
using TestFirstProject.Exceptions;
using TestFirstProject.Models;
using TestFirstProject.Models.Enums;
using TestFirstProject.Services.Interfaces;

namespace TestFirstProject.Services.Implementations
{
    public class PaymentService : IPaymentService
    {
        private readonly PersonsContext _context;
        private readonly IEnumerable<IPaymentGateway> _gateways;

        public PaymentService(PersonsContext context, IEnumerable<IPaymentGateway> gateways)
        {
            _context = context;
            _gateways = gateways;
        }

        public async Task<PaymentResponse> InitiatePaymentAsync(InitiatePaymentRequest request, Guid userId)
        {
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == request.OrderId && o.UserId == userId)
                ?? throw new NotFoundException($"Order '{request.OrderId}' not found.");

            if (order.Status != OrderStatus.Pending && order.Status != OrderStatus.Confirmed)
                throw new ValidationException($"Order must be in Pending or Confirmed status to initiate payment. Current status: {order.Status}.");

            if (!Enum.TryParse<PaymentMethod>(request.PaymentMethod, true, out var method))
                throw new ValidationException($"Invalid payment method: '{request.PaymentMethod}'.");

            if (method == PaymentMethod.Wallet)
            {
                var wallet = await GetOrCreateWalletAsync(userId);
                if (wallet.Balance < order.TotalAmount)
                    throw new ValidationException("Insufficient wallet balance.");
            }

            var gateway = ResolveGateway(method);

            var payment = new Payment
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                Method = method,
                Status = PaymentStatus.Pending,
                Amount = order.TotalAmount,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Payments.Add(payment);

            var (success, transactionId, failureReason) = await gateway.AuthorizeAsync(payment.Amount);

            _context.PaymentAttempts.Add(new PaymentAttempt
            {
                Id = Guid.NewGuid(),
                PaymentId = payment.Id,
                Status = success ? PaymentStatus.Authorized : PaymentStatus.Failed,
                FailureReason = failureReason,
                GatewayResponse = transactionId,
                AttemptedAt = DateTime.UtcNow
            });

            if (success)
            {
                payment.Status = PaymentStatus.Authorized;
                payment.TransactionId = transactionId;
            }
            else
            {
                payment.Status = PaymentStatus.Failed;
            }

            payment.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return MapToResponse(payment);
        }

        public async Task<PaymentResponse> CapturePaymentAsync(Guid paymentId, Guid userId)
        {
            var payment = await LoadPaymentWithOrderAsync(paymentId, userId);

            if (payment.Status != PaymentStatus.Authorized)
                throw new ValidationException($"Payment must be in Authorized status to capture. Current status: {payment.Status}.");

            var gateway = ResolveGateway(payment.Method);
            var (success, failureReason) = await gateway.CaptureAsync(payment.TransactionId!, payment.Amount);

            if (!success)
                throw new AppException($"Capture failed: {failureReason}");

            payment.Status = PaymentStatus.Captured;
            payment.UpdatedAt = DateTime.UtcNow;

            if (payment.Method == PaymentMethod.Wallet)
            {
                var wallet = await GetOrCreateWalletAsync(userId);
                wallet.Balance -= payment.Amount;
                wallet.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return MapToResponse(payment);
        }

        public async Task<PaymentResponse> RefundPaymentAsync(Guid paymentId, RefundPaymentRequest request, Guid userId)
        {
            var payment = await LoadPaymentWithOrderAsync(paymentId, userId);

            if (payment.Status != PaymentStatus.Captured)
                throw new ValidationException($"Payment must be in Captured status to refund. Current status: {payment.Status}.");

            var refundableBalance = payment.Amount - payment.RefundedAmount;
            var refundAmount = request.Amount ?? refundableBalance;

            if (refundAmount <= 0)
                throw new ValidationException("Refund amount must be greater than zero.");

            if (refundAmount > refundableBalance)
                throw new ValidationException($"Refund amount ({refundAmount}) exceeds the refundable balance ({refundableBalance}).");

            var gateway = ResolveGateway(payment.Method);
            var (success, failureReason) = await gateway.RefundAsync(payment.TransactionId!, refundAmount);

            if (!success)
                throw new AppException($"Refund failed: {failureReason}");

            payment.RefundedAmount += refundAmount;

            if (payment.RefundedAmount >= payment.Amount)
                payment.Status = PaymentStatus.Refunded;

            payment.UpdatedAt = DateTime.UtcNow;

            if (payment.Method == PaymentMethod.Wallet)
            {
                var wallet = await GetOrCreateWalletAsync(userId);
                wallet.Balance += refundAmount;
                wallet.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return MapToResponse(payment);
        }

        public async Task<WalletResponse> GetWalletBalanceAsync(Guid userId)
        {
            var wallet = await GetOrCreateWalletAsync(userId);
            return new WalletResponse(wallet.Balance, wallet.UpdatedAt);
        }

        public async Task<WalletTopUpResponse> TopUpWalletAsync(Guid userId, TopUpWalletRequest request)
        {
            if (request.Amount <= 0)
                throw new ValidationException("Top-up amount must be greater than zero.");

            var wallet = await GetOrCreateWalletAsync(userId);
            wallet.Balance += request.Amount;
            wallet.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new WalletTopUpResponse(wallet.Balance, request.Amount);
        }

        // --- Private Helpers ---

        private IPaymentGateway ResolveGateway(PaymentMethod method)
        {
            return _gateways.FirstOrDefault(g => g.SupportedMethod == method)
                ?? throw new AppException($"No gateway registered for payment method '{method}'.");
        }

        private async Task<Payment> LoadPaymentWithOrderAsync(Guid paymentId, Guid userId)
        {
            var payment = await _context.Payments
                .Include(p => p.Order)
                .FirstOrDefaultAsync(p => p.Id == paymentId && p.Order.UserId == userId)
                ?? throw new NotFoundException($"Payment '{paymentId}' not found.");

            return payment;
        }

        private async Task<Wallet> GetOrCreateWalletAsync(Guid userId)
        {
            var wallet = await _context.Wallets.FirstOrDefaultAsync(w => w.UserId == userId);
            if (wallet != null)
                return wallet;

            wallet = new Wallet
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Balance = 0,
                UpdatedAt = DateTime.UtcNow
            };
            _context.Wallets.Add(wallet);
            await _context.SaveChangesAsync();

            return wallet;
        }

        private static PaymentResponse MapToResponse(Payment payment)
        {
            return new PaymentResponse(
                payment.Id,
                payment.OrderId,
                payment.Method.ToString(),
                payment.Status.ToString(),
                payment.Amount,
                payment.RefundedAmount,
                payment.TransactionId,
                payment.CreatedAt);
        }
    }
}
