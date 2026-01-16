using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Cart;

/// <summary>
/// Partial update DTO for Cart Item (PATCH support)
/// All fields are optional for partial updates
/// HIGH-API-001: PATCH Support - Partial updates without requiring all fields
/// </summary>
public record PatchCartItemDto
{
    [Range(1, int.MaxValue, ErrorMessage = "Miktar 1'den büyük olmalıdır.")]
    public int? Quantity { get; init; }
}
