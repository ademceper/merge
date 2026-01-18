using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Seller;

public record RequestPayoutDto
{
    [Required]
    [MinLength(1, ErrorMessage = "En az bir komisyon seçilmelidir.")]
    public List<Guid> CommissionIds { get; init; } = new();
}
