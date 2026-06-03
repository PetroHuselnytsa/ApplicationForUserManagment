using TestFirstProject.DTOs;
using TestFirstProject.Extensions;
using TestFirstProject.Services.Interfaces;

namespace TestFirstProject.Endpoints
{
    /// <summary>
    /// Maps book lending endpoints.
    /// </summary>
    public static class LoanEndpoints
    {
        public static void MapLoanEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/loans")
                .WithTags("Loans")
                .RequireAuthorization();

            // POST /loans/checkout - checkout a book
            group.MapPost("/checkout", async (
                CheckoutBookRequest request,
                ILoanService loanService,
                HttpContext httpContext) =>
            {
                var userId = httpContext.GetUserId();
                var result = await loanService.CheckoutBookAsync(userId, request.BookId);
                return Results.Created($"/api/loans/{result.Id}", result);
            })
            .WithName("CheckoutBook");

            // POST /loans/{id}/return - return a book
            group.MapPost("/{id:guid}/return", async (
                Guid id,
                ReturnBookRequest? request,
                ILoanService loanService,
                HttpContext httpContext) =>
            {
                var userId = httpContext.GetUserId();
                var result = await loanService.ReturnBookAsync(userId, id, request?.Notes);
                return Results.Ok(result);
            })
            .WithName("ReturnBook");

            // GET /loans/my - get current user's loans
            group.MapGet("/my", async (
                ILoanService loanService,
                HttpContext httpContext,
                bool activeOnly = false) =>
            {
                var userId = httpContext.GetUserId();
                var result = await loanService.GetMyLoansAsync(userId, activeOnly);
                return Results.Ok(result);
            })
            .WithName("GetMyLoans");

            // GET /loans - get all loans (Admin only)
            group.MapGet("/", async (ILoanService loanService, bool activeOnly = false) =>
            {
                var result = await loanService.GetAllLoansAsync(activeOnly);
                return Results.Ok(result);
            })
            .WithName("GetAllLoans")
            .RequireAuthorization(policy => policy.RequireRole("Admin"));

            // GET /loans/overdue - get overdue loans (Admin only)
            group.MapGet("/overdue", async (ILoanService loanService) =>
            {
                var result = await loanService.GetOverdueLoansAsync();
                return Results.Ok(result);
            })
            .WithName("GetOverdueLoans")
            .RequireAuthorization(policy => policy.RequireRole("Admin"));
        }
    }
}
