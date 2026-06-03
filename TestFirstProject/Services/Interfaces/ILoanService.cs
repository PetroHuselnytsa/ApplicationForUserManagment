using TestFirstProject.DTOs;

namespace TestFirstProject.Services.Interfaces
{
    /// <summary>
    /// Handles book lending and return operations.
    /// </summary>
    public interface ILoanService
    {
        Task<LoanDto> CheckoutBookAsync(Guid userId, Guid bookId);
        Task<LoanDto> ReturnBookAsync(Guid userId, Guid loanId, string? notes = null);
        Task<IReadOnlyList<MyLoanDto>> GetMyLoansAsync(Guid userId, bool activeOnly = false);
        Task<IReadOnlyList<LoanDto>> GetAllLoansAsync(bool activeOnly = false);
        Task<IReadOnlyList<LoanDto>> GetOverdueLoansAsync();
    }
}
