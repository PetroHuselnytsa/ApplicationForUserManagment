namespace TestFirstProject.Models;

/// <summary>
/// Physical warehouse location for multi-warehouse inventory.
/// </summary>
public class Warehouse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    // Navigation
    public ICollection<StockEntry> StockEntries { get; set; } = new List<StockEntry>();
}
