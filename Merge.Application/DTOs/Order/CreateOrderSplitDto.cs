using System.ComponentModel.DataAnnotations;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.DTOs.Order;

public class CreateOrderSplitDto
{
    [Required]
    public Guid OrderId { get; set; }
    
    [Required]
    [StringLength(500, MinimumLength = 5, ErrorMessage = "Bölme nedeni en az 5, en fazla 500 karakter olmalıdır.")]
    public string SplitReason { get; set; } = string.Empty;
    
    public Guid? NewAddressId { get; set; }
    
    [Required]
    [MinLength(1, ErrorMessage = "En az bir ürün seçilmelidir.")]
    public List<SplitOrderItemRequest> Items { get; set; } = new();
}
