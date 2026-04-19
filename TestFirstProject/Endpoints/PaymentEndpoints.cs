using TestFirstProject.DTOs;
using TestFirstProject.Extensions;
using TestFirstProject.Services.Interfaces;

namespace TestFirstProject.Endpoints
{
    public static class PaymentEndpoints
    {
        public static void MapPaymentEndpoints(this IEndpointRouteBuilder app)
        {
            var payments = app.MapGroup("/api/payments")
                              .WithTags("Payments")
                              .RequireAuthorization();

            payments.MapPost("/initiate", async (InitiatePaymentRequest request, IPaymentService paymentService, HttpContext httpContext) =>
            {
                var userId = httpContext.GetUserId();
                var result = await paymentService.InitiatePaymentAsync(request, userId);
                return Results.Ok(result);
            })
            .WithName("InitiatePayment");

            payments.MapPost("/{id:guid}/capture", async (Guid id, IPaymentService paymentService, HttpContext httpContext) =>
            {
                var userId = httpContext.GetUserId();
                var result = await paymentService.CapturePaymentAsync(id, userId);
                return Results.Ok(result);
            })
            .WithName("CapturePayment");

            payments.MapPost("/{id:guid}/refund", async (Guid id, RefundPaymentRequest request, IPaymentService paymentService, HttpContext httpContext) =>
            {
                var userId = httpContext.GetUserId();
                var result = await paymentService.RefundPaymentAsync(id, request, userId);
                return Results.Ok(result);
            })
            .WithName("RefundPayment");

            var wallet = app.MapGroup("/api/wallet")
                            .WithTags("Wallet")
                            .RequireAuthorization();

            wallet.MapGet("/balance", async (IPaymentService paymentService, HttpContext httpContext) =>
            {
                var userId = httpContext.GetUserId();
                var result = await paymentService.GetWalletBalanceAsync(userId);
                return Results.Ok(result);
            })
            .WithName("GetWalletBalance");

            wallet.MapPost("/topup", async (TopUpWalletRequest request, IPaymentService paymentService, HttpContext httpContext) =>
            {
                var userId = httpContext.GetUserId();
                var result = await paymentService.TopUpWalletAsync(userId, request);
                return Results.Ok(result);
            })
            .WithName("TopUpWallet");
        }
    }
}
