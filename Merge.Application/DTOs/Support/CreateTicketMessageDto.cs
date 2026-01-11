using System.ComponentModel.DataAnnotations;
namespace Merge.Application.DTOs.Support;

public class CreateTicketMessageDto
{
    [Required]
    public Guid TicketId { get; set; }

    [Required]
    // ✅ BOLUM 12.0: Magic Number'ları Configuration'a Taşıma - DTO validation matches SupportSettings.MaxTicketMessageLength=10000
    [StringLength(10000, MinimumLength = 1, ErrorMessage = "Mesaj boş olamaz ve en fazla 10000 karakter olmalıdır.")]
    public string Message { get; set; } = string.Empty;

    public bool IsInternal { get; set; } = false;
}
