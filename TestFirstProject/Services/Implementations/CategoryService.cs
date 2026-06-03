using Microsoft.EntityFrameworkCore;
using TestFirstProject.Contexts;
using TestFirstProject.DTOs;
using TestFirstProject.Exceptions;
using TestFirstProject.Models;
using TestFirstProject.Services.Interfaces;

namespace TestFirstProject.Services.Implementations
{
    public class CategoryService : ICategoryService
    {
        private readonly PersonsContext _context;

        public CategoryService(PersonsContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<CategoryDto>> GetAllCategoriesAsync()
        {
            var categories = await _context.Categories
                .AsNoTracking()
                .Include(c => c.BookCategories)
                .OrderBy(c => c.Name)
                .ToListAsync();

            return categories.Select(MapToCategoryDto).ToList();
        }

        public async Task<CategoryDto> GetCategoryByIdAsync(Guid id)
        {
            var category = await _context.Categories
                .AsNoTracking()
                .Include(c => c.BookCategories)
                .FirstOrDefaultAsync(c => c.Id == id)
                ?? throw new NotFoundException("Category not found.");

            return MapToCategoryDto(category);
        }

        public async Task<CategoryDto> CreateCategoryAsync(CreateCategoryRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                throw new ValidationException("Name is required.");

            var exists = await _context.Categories.AnyAsync(c => c.Name == request.Name);
            if (exists)
                throw new ConflictException("A category with this name already exists.");

            var category = new Category
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description,
                CreatedAt = DateTime.UtcNow
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return MapToCategoryDto(category);
        }

        public async Task<CategoryDto> UpdateCategoryAsync(Guid id, UpdateCategoryRequest request)
        {
            var category = await _context.Categories
                .Include(c => c.BookCategories)
                .FirstOrDefaultAsync(c => c.Id == id)
                ?? throw new NotFoundException("Category not found.");

            if (string.IsNullOrWhiteSpace(request.Name))
                throw new ValidationException("Name is required.");

            if (request.Name != category.Name)
            {
                var exists = await _context.Categories.AnyAsync(c => c.Name == request.Name && c.Id != id);
                if (exists)
                    throw new ConflictException("A category with this name already exists.");
            }

            category.Name = request.Name;
            category.Description = request.Description;

            await _context.SaveChangesAsync();
            return MapToCategoryDto(category);
        }

        public async Task DeleteCategoryAsync(Guid id)
        {
            var category = await _context.Categories.FindAsync(id)
                ?? throw new NotFoundException("Category not found.");

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
        }

        private static CategoryDto MapToCategoryDto(Category category)
        {
            return new CategoryDto(
                category.Id,
                category.Name,
                category.Description,
                category.CreatedAt,
                category.BookCategories.Count
            );
        }
    }
}
