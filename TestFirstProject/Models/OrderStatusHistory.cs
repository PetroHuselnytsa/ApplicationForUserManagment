using TestFirstProject.Models.Enums;

namespace TestFirstProject.Models
{
    /// <summary>
    /// Records every status change on an order with timestamp and actor.
    /// </summary>
    public class OrderStatusHistory
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public Order Order { get; set; } = null!;

        public OrderStatus FromStatus { get; set; }
        public OrderStatus ToStatus { get; set; }

        public Guid ChangedByUserId { get; set; }
        public AppUser ChangedByUser { get; set; } = null!;

        public string? Notes { get; set; }
        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
    }
}
