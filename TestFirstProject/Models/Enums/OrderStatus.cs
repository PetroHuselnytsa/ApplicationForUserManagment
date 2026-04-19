namespace TestFirstProject.Models.Enums
{
    /// <summary>
    /// Represents every state in the order lifecycle.
    /// </summary>
    public enum OrderStatus
    {
        Draft = 0,
        Pending = 1,
        Confirmed = 2,
        Processing = 3,
        Shipped = 4,
        Delivered = 5,
        Cancelled = 6,
        Refunded = 7
    }
}
