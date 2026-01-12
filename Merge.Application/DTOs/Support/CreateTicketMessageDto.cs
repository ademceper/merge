using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Support;

/// <summary>
/// ✅ BOLUM 7.1.5: Records - DTO'lar record olmalı (ZORUNLU)
/// </summary>
public record CreateTicketMessageDto
{
    [Required]
    public Guid TicketId { get; init; }

    [Required]
    // ✅ BOLUM 12.0: Magic Number'ları Configuration'a Taşıma - DTO validation matches SupportSettings.MaxTicketMessageLength=10000
    [StringLength(10000, MinimumLength = 1, ErrorMessage = "Mesaj boş olamaz ve en fazla 10000 karakter olmalıdır.")]
    public string Message { get; init; } = string.Empty;

    public bool IsInternal { get; init; } = false;
}
