using TestFirstProject.DTOs.Payments;
using TestFirstProject.Extensions;
using TestFirstProject.Services.Interfaces;

namespace TestFirstProject.Endpoints;

public static class PaymentEndpoints
{
    public static void MapPaymentEndpoints(this WebApplication app)
    {
        // Payment endpoints
        var paymentGroup = app.MapGroup("/api/payments")
            .RequireAuthorization();

        // POST /api/payments/initiate — start payment for an order
        paymentGroup.MapPost("/initiate", async (
            InitiatePaymentRequest request,
            IPaymentService paymentService,
            HttpContext context) =>
        {
            var userId = context.GetUserId();
            var payment = await paymentService.InitiatePaymentAsync(request, userId);
            return Results.Created($"/api/payments/{payment.Id}", payment);
        });

        // POST /api/payments/{id}/capture — capture authorized payment
        paymentGroup.MapPost("/{id:guid}/capture", async (
            Guid id,
            IPaymentService paymentService,
            HttpContext context) =>
        {
            var userId = context.GetUserId();
            var payment = await paymentService.CapturePaymentAsync(id, userId);
            return Results.Ok(payment);
        });

        // POST /api/payments/{id}/refund — initiate refund
        paymentGroup.MapPost("/{id:guid}/refund", async (
            Guid id,
            RefundRequest request,
            IPaymentService paymentService,
            HttpContext context) =>
        {
            var userId = context.GetUserId();
            var payment = await paymentService.RefundPaymentAsync(id, request, userId);
            return Results.Ok(payment);
        });

        // Wallet endpoints
        var walletGroup = app.MapGroup("/api/wallet")
            .RequireAuthorization();

        // GET /api/wallet/balance — get wallet balance
        walletGroup.MapGet("/balance", async (IPaymentService paymentService, HttpContext context) =>
        {
            var userId = context.GetUserId();
            var wallet = await paymentService.GetWalletBalanceAsync(userId);
            return Results.Ok(wallet);
        });

        // POST /api/wallet/topup — add funds to wallet
        walletGroup.MapPost("/topup", async (
            WalletTopUpRequest request,
            IPaymentService paymentService,
            HttpContext context) =>
        {
            var userId = context.GetUserId();
            var wallet = await paymentService.TopUpWalletAsync(userId, request);
            return Results.Ok(wallet);
        });
    }
}
