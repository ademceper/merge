using System.ComponentModel.DataAnnotations;
namespace Merge.Application.DTOs.Support;

public class CreateTicketMessageDto
{
    [Required]
    public Guid TicketId { get; set; }

    [Required]
    [StringLength(5000, MinimumLength = 1, ErrorMessage = "Mesaj boş olamaz ve en fazla 5000 karakter olmalıdır.")]
    public string Message { get; set; } = string.Empty;

    public bool IsInternal { get; set; } = false;
}
