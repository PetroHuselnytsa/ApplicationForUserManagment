namespace TestFirstProject.Models;

/// <summary>
/// Hierarchical product category supporting parent/child relationships.
/// </summary>
public class Category
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    // Self-referencing hierarchy
    public Guid? ParentCategoryId { get; set; }
    public Category? ParentCategory { get; set; }
    public ICollection<Category> SubCategories { get; set; } = new List<Category>();

    // Navigation
    public ICollection<Product> Products { get; set; } = new List<Product>();
}
