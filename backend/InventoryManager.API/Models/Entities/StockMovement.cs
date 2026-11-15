using System.ComponentModel.DataAnnotations;
using InventoryManager.API.Models.Common;
using InventoryManager.API.Models.Enums;

namespace InventoryManager.API.Models.Entities;

public class StockMovement : BaseAuditableEntity
{
    [Required]
    public int InventoryItemId { get; set; }

    public InventoryItem? InventoryItem { get; set; }

    [Required]
    public StockMovementType Type { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than zero.")]
    public int Quantity { get; set; }

    [MaxLength(500)]
    public string? Note { get; set; }
}
