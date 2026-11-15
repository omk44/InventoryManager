using System.ComponentModel.DataAnnotations;
using InventoryManager.API.Models.Common;

namespace InventoryManager.API.Models.Entities;

public class Category : BaseAuditableEntity
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public ICollection<InventoryItem> InventoryItems { get; set; } = new List<InventoryItem>();
}
