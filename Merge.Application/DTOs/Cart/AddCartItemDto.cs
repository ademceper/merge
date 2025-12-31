using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Cart;

public class AddCartItemDto
{
    [Required]
    public Guid ProductId { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Miktar 1'den büyük olmalıdır.")]
    public int Quantity { get; set; }
}

