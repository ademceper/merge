using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Logistics;

public record UpdateShippingStatusDto(
    [Required]
    [StringLength(50)]
    string Status
);
