using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Order;

public class SplitOrderItemRequest
{
    [Required]
    public Guid OrderItemId { get; set; }
    
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Miktar 1 veya daha büyük olmalıdır.")]
    public int Quantity { get; set; }
}

