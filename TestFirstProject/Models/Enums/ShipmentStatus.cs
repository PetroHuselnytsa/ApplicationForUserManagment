namespace TestFirstProject.Models.Enums
{
    /// <summary>
    /// Tracking status for shipments.
    /// </summary>
    public enum ShipmentStatus
    {
        Pending = 0,
        PickedUp = 1,
        InTransit = 2,
        OutForDelivery = 3,
        Delivered = 4,
        ReturnedToSender = 5
    }
}
