using InventoryManager.API.Data;
using InventoryManager.API.DTOs.Category;
using InventoryManager.API.DTOs.Common;
using InventoryManager.API.Models.Entities;
using InventoryManager.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InventoryManager.API.Services.Implementations;

public class CategoryService : ICategoryService
{
    private readonly ApplicationDbContext _context;

    public CategoryService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<IEnumerable<CategoryDto>>> GetAllCategoriesAsync()
    {
        var categories = await _context.Categories
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                CreatedAt = c.CreatedAt,
                CreatedBy = c.CreatedBy,
                ItemCount = c.InventoryItems.Count
            })
            .ToListAsync();

        return ApiResponse<IEnumerable<CategoryDto>>.Ok(categories);
    }

    public async Task<ApiResponse<CategoryDto>> GetCategoryByIdAsync(int id)
    {
        var category = await _context.Categories
            .AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                CreatedAt = c.CreatedAt,
                CreatedBy = c.CreatedBy,
                ItemCount = c.InventoryItems.Count
            })
            .FirstOrDefaultAsync();

        if (category == null)
        {
            return ApiResponse<CategoryDto>.Fail("Category not found.");
        }

        return ApiResponse<CategoryDto>.Ok(category);
    }

    public async Task<ApiResponse<CategoryDto>> CreateCategoryAsync(CreateCategoryDto dto, string currentUserName)
    {
        var normalizedName = dto.Name.Trim();

        var exists = await _context.Categories
            .AnyAsync(c => c.Name.ToLower() == normalizedName.ToLower());

        if (exists)
        {
            return ApiResponse<CategoryDto>.Fail($"A category with the name '{normalizedName}' already exists.");
        }

        var category = new Category
        {
            Name = normalizedName,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = currentUserName
        };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        var resultDto = new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            CreatedAt = category.CreatedAt,
            CreatedBy = category.CreatedBy,
            ItemCount = 0
        };

        return ApiResponse<CategoryDto>.Ok(resultDto, "Category created successfully.");
    }

    public async Task<ApiResponse<CategoryDto>> UpdateCategoryAsync(int id, UpdateCategoryDto dto, string currentUserName)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category == null)
        {
            return ApiResponse<CategoryDto>.Fail("Category not found.");
        }

        var normalizedName = dto.Name.Trim();

        var duplicateExists = await _context.Categories
            .AnyAsync(c => c.Id != id && c.Name.ToLower() == normalizedName.ToLower());

        if (duplicateExists)
        {
            return ApiResponse<CategoryDto>.Fail($"Another category with the name '{normalizedName}' already exists.");
        }

        category.Name = normalizedName;
        category.UpdatedAt = DateTime.UtcNow;
        category.UpdatedBy = currentUserName;

        await _context.SaveChangesAsync();

        var itemCount = await _context.InventoryItems.CountAsync(i => i.CategoryId == id);

        var resultDto = new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            CreatedAt = category.CreatedAt,
            CreatedBy = category.CreatedBy,
            ItemCount = itemCount
        };

        return ApiResponse<CategoryDto>.Ok(resultDto, "Category updated successfully.");
    }

    public async Task<ApiResponse<bool>> DeleteCategoryAsync(int id)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category == null)
        {
            return ApiResponse<bool>.Fail("Category not found.");
        }

        var hasItems = await _context.InventoryItems.AnyAsync(i => i.CategoryId == id);
        if (hasItems)
        {
            return ApiResponse<bool>.Fail("Cannot delete this category because it has associated inventory items. Reassign or remove items first.");
        }

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();

        return ApiResponse<bool>.Ok(true, "Category deleted successfully.");
    }
}
