using Microsoft.EntityFrameworkCore;
using TestFirstProject.Contexts;
using TestFirstProject.DTOs;
using TestFirstProject.Exceptions;
using TestFirstProject.Models;
using TestFirstProject.Models.Enums;
using TestFirstProject.Services.Interfaces;

namespace TestFirstProject.Services.Implementations
{
    public class LoanService : ILoanService
    {
        private readonly PersonsContext _context;
        private const int DefaultLoanDays = 14;

        public LoanService(PersonsContext context)
        {
            _context = context;
        }

        public async Task<LoanDto> CheckoutBookAsync(Guid userId, Guid bookId)
        {
            var book = await _context.Books.FindAsync(bookId)
                ?? throw new NotFoundException("Book not found.");

            if (book.AvailableCopies < 1)
                throw new ConflictException("No copies available for checkout.");

            var existingLoan = await _context.Loans.AnyAsync(l =>
                l.BookId == bookId && l.BorrowerId == userId && l.Status == LoanStatus.Active);
            if (existingLoan)
                throw new ConflictException("You already have an active loan for this book.");

            var loan = new Loan
            {
                Id = Guid.NewGuid(),
                BookId = bookId,
                BorrowerId = userId,
                CheckoutDate = DateTime.UtcNow,
                DueDate = DateTime.UtcNow.AddDays(DefaultLoanDays),
                Status = LoanStatus.Active
            };

            book.AvailableCopies--;

            _context.Loans.Add(loan);
            await _context.SaveChangesAsync();

            return await GetLoanByIdAsync(loan.Id);
        }

        public async Task<LoanDto> ReturnBookAsync(Guid userId, Guid loanId, string? notes = null)
        {
            var loan = await _context.Loans
                .Include(l => l.Book)
                .FirstOrDefaultAsync(l => l.Id == loanId)
                ?? throw new NotFoundException("Loan not found.");

            if (loan.BorrowerId != userId)
                throw new ForbiddenException("You can only return your own loans.");

            if (loan.Status == LoanStatus.Returned)
                throw new ValidationException("This book has already been returned.");

            loan.ReturnDate = DateTime.UtcNow;
            loan.Status = LoanStatus.Returned;
            loan.Notes = notes;

            loan.Book.AvailableCopies++;

            await _context.SaveChangesAsync();

            return await GetLoanByIdAsync(loan.Id);
        }

        public async Task<IReadOnlyList<MyLoanDto>> GetMyLoansAsync(Guid userId, bool activeOnly = false)
        {
            var query = _context.Loans
                .AsNoTracking()
                .Include(l => l.Book)
                .Where(l => l.BorrowerId == userId);

            if (activeOnly)
                query = query.Where(l => l.Status == LoanStatus.Active);

            var loans = await query
                .OrderByDescending(l => l.CheckoutDate)
                .ToListAsync();

            return loans.Select(l => new MyLoanDto(
                l.Id,
                l.BookId,
                l.Book.Title,
                l.Book.Isbn,
                l.CheckoutDate,
                l.DueDate,
                l.ReturnDate,
                l.Status.ToString()
            )).ToList();
        }

        public async Task<IReadOnlyList<LoanDto>> GetAllLoansAsync(bool activeOnly = false)
        {
            var query = _context.Loans
                .AsNoTracking()
                .Include(l => l.Book)
                .Include(l => l.Borrower);

            if (activeOnly)
                query = query.Where(l => l.Status == LoanStatus.Active);

            var loans = await query
                .OrderByDescending(l => l.CheckoutDate)
                .ToListAsync();

            return loans.Select(MapToLoanDto).ToList();
        }

        public async Task<IReadOnlyList<LoanDto>> GetOverdueLoansAsync()
        {
            var now = DateTime.UtcNow;
            var loans = await _context.Loans
                .AsNoTracking()
                .Include(l => l.Book)
                .Include(l => l.Borrower)
                .Where(l => l.Status == LoanStatus.Active && l.DueDate < now)
                .OrderBy(l => l.DueDate)
                .ToListAsync();

            return loans.Select(MapToLoanDto).ToList();
        }

        private async Task<LoanDto> GetLoanByIdAsync(Guid id)
        {
            var loan = await _context.Loans
                .AsNoTracking()
                .Include(l => l.Book)
                .Include(l => l.Borrower)
                .FirstOrDefaultAsync(l => l.Id == id)
                ?? throw new NotFoundException("Loan not found.");

            return MapToLoanDto(loan);
        }

        private static LoanDto MapToLoanDto(Loan loan)
        {
            return new LoanDto(
                loan.Id,
                loan.BookId,
                loan.Book.Title,
                loan.Book.Isbn,
                loan.BorrowerId,
                loan.Borrower.Username,
                loan.CheckoutDate,
                loan.DueDate,
                loan.ReturnDate,
                loan.Status.ToString(),
                loan.Notes
            );
        }
    }
}
