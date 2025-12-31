using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.B2B;

public class CreatePurchaseOrderItemDto
{
    [Required]
    public Guid ProductId { get; set; }
    
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Miktar en az 1 olmalıdır.")]
    public int Quantity { get; set; }
    
    [StringLength(500)]
    public string? Notes { get; set; }
}
