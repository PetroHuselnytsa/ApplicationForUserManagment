namespace TestFirstProject.DTOs
{
    // --- Loans ---

    public record CheckoutBookRequest(Guid BookId);

    public record ReturnBookRequest(string? Notes);

    public record LoanDto(
        Guid Id,
        Guid BookId,
        string BookTitle,
        string BookIsbn,
        Guid BorrowerId,
        string BorrowerUsername,
        DateTime CheckoutDate,
        DateTime DueDate,
        DateTime? ReturnDate,
        string Status,
        string? Notes
    );

    public record MyLoanDto(
        Guid Id,
        Guid BookId,
        string BookTitle,
        string BookIsbn,
        DateTime CheckoutDate,
        DateTime DueDate,
        DateTime? ReturnDate,
        string Status
    );
}
