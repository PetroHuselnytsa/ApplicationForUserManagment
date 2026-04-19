namespace TestFirstProject.Models
{
    /// <summary>
    /// Represents a physical warehouse location.
    /// </summary>
    public class Warehouse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string Location { get; set; } = null!;
        public bool IsActive { get; set; } = true;

        public ICollection<StockEntry> StockEntries { get; set; } = new List<StockEntry>();
    }
}
