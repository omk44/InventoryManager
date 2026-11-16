using InventoryManager.API.DTOs.Common;

namespace InventoryManager.API.DTOs.Inventory;

public class InventoryItemFilterParams : PaginationParams
{
    public int? CategoryId { get; set; }
    public bool? LowStockOnly { get; set; }
}