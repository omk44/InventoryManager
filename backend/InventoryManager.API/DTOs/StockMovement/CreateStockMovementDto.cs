using System.ComponentModel.DataAnnotations;
using InventoryManager.API.Models.Enums;

namespace InventoryManager.API.DTOs.StockMovement;

public class CreateStockMovementDto
{
    [Required(ErrorMessage = "InventoryItemId is required.")]
    public int InventoryItemId { get; set; }

    [Required(ErrorMessage = "Movement type is required.")]
    public StockMovementType Type { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0.")]
    public int Quantity { get; set; }

    [MaxLength(500, ErrorMessage = "Note cannot exceed 500 characters.")]
    public string? Note { get; set; }
}