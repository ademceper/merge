using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Support;


public record CreateTicketMessageDto
{
    [Required]
    public Guid TicketId { get; init; }

    [Required]
    [StringLength(10000, MinimumLength = 1, ErrorMessage = "Mesaj boş olamaz ve en fazla 10000 karakter olmalıdır.")]
    public string Message { get; init; } = string.Empty;

    public bool IsInternal { get; init; } = false;
}
