using InventoryManager.API.DTOs.Common;
using InventoryManager.API.DTOs.StockMovement;

namespace InventoryManager.API.Services.Interfaces;

public interface IStockMovementService
{
    Task<ApiResponse<IEnumerable<StockMovementDto>>> GetMovementsByItemIdAsync(int inventoryItemId);
    Task<ApiResponse<StockMovementDto>> RecordMovementAsync(CreateStockMovementDto dto);
}
