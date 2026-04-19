namespace TestFirstProject.Models.Enums
{
    /// <summary>
    /// Status of a restock request.
    /// </summary>
    public enum RestockRequestStatus
    {
        Pending = 0,
        Approved = 1,
        Fulfilled = 2,
        Cancelled = 3
    }
}
