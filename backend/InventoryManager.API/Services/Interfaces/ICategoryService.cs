using InventoryManager.API.DTOs.Category;
using InventoryManager.API.DTOs.Common;

namespace InventoryManager.API.Services.Interfaces;

public interface ICategoryService
{
    Task<ApiResponse<IEnumerable<CategoryDto>>> GetAllCategoriesAsync();
    Task<ApiResponse<CategoryDto>> GetCategoryByIdAsync(int id);
    Task<ApiResponse<CategoryDto>> CreateCategoryAsync(CreateCategoryDto dto, string currentUserName);
    Task<ApiResponse<CategoryDto>> UpdateCategoryAsync(int id, UpdateCategoryDto dto, string currentUserName);
    Task<ApiResponse<bool>> DeleteCategoryAsync(int id);
}