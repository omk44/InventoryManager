using System.ComponentModel.DataAnnotations;
using InventoryManager.API.Models.Common;

namespace InventoryManager.API.Models.Entities;

public class InventoryItem : BaseAuditableEntity
{
    [Required]
    [MaxLength(100)]
    public string SKU { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [Required]
    public int CategoryId { get; set; }

    public Category? Category { get; set; }

    public int ReorderLevel { get; set; } = 0;

    [Required]
    public string OwnerUserId { get; set; } = string.Empty;

    public ApplicationUser? OwnerUser { get; set; }

    public ICollection<StockMovement> StockMovements { get; set; } = new List<StockMovement>();
}
