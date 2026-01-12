using System.ComponentModel.DataAnnotations;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.DTOs.Order;

public class CreateReturnRequestDto
{
    [Required]
    public Guid OrderId { get; set; }
    
    [Required]
    public Guid UserId { get; set; }
    
    [Required]
    [StringLength(1000, MinimumLength = 5, ErrorMessage = "İade nedeni en az 5, en fazla 1000 karakter olmalıdır.")]
    public string Reason { get; set; } = string.Empty;
    
    [Required]
    [MinLength(1, ErrorMessage = "En az bir ürün seçilmelidir.")]
    public List<Guid> OrderItemIds { get; set; } = new List<Guid>();
}
