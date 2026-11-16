using InventoryManager.API.DTOs.Common;
using InventoryManager.API.DTOs.Inventory;

namespace InventoryManager.API.Services.Interfaces;

public interface IInventoryService
{
    Task<ApiResponse<PagedResult<InventoryItemDto>>> GetItemsAsync(InventoryItemFilterParams filterParams);
    Task<ApiResponse<InventoryItemDto>> GetItemByIdAsync(int id);
    Task<ApiResponse<InventoryItemDto>> CreateItemAsync(CreateInventoryItemDto dto);
    Task<ApiResponse<InventoryItemDto>> UpdateItemAsync(int id, UpdateInventoryItemDto dto);
    Task<ApiResponse<bool>> DeleteItemAsync(int id);
    Task<int> GetCurrentStockAsync(int inventoryItemId);
}
