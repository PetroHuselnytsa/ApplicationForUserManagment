namespace TestFirstProject.Models.DTOs
{
    public class ApiErrorResponse
    {
        public string Message { get; set; } = null!;

        public IEnumerable<string>? Errors { get; set; }
    }
}
