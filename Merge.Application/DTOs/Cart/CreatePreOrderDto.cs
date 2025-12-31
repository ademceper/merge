using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Cart;

public class CreatePreOrderDto
{
    [Required]
    public Guid ProductId { get; set; }
    
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Miktar en az 1 olmalıdır.")]
    public int Quantity { get; set; } = 1;
    
    [StringLength(500)]
    public string? VariantOptions { get; set; }
    
    [StringLength(1000)]
    public string? Notes { get; set; }
}
