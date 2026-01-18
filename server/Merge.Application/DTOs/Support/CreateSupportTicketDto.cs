using System.ComponentModel.DataAnnotations;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.DTOs.Support;


public record CreateSupportTicketDto
{
    [Required]
    [StringLength(50)]
    public string Category { get; init; } = string.Empty;

    [StringLength(20)]
    public string Priority { get; init; } = "Medium";

    [Required]
    [StringLength(200, MinimumLength = 5, ErrorMessage = "Konu en az 5, en fazla 200 karakter olmalıdır.")]
    public string Subject { get; init; } = string.Empty;

    [Required]
    [StringLength(5000, MinimumLength = 10, ErrorMessage = "Açıklama en az 10, en fazla 5000 karakter olmalıdır.")]
    public string Description { get; init; } = string.Empty;

    public Guid? OrderId { get; init; }

    public Guid? ProductId { get; init; }
}
