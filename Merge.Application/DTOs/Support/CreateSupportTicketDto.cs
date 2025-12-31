using System.ComponentModel.DataAnnotations;
namespace Merge.Application.DTOs.Support;

public class CreateSupportTicketDto
{
    [Required]
    [StringLength(50)]
    public string Category { get; set; } = string.Empty;

    [StringLength(20)]
    public string Priority { get; set; } = "Medium";

    [Required]
    [StringLength(200, MinimumLength = 5, ErrorMessage = "Konu en az 5, en fazla 200 karakter olmalıdır.")]
    public string Subject { get; set; } = string.Empty;

    [Required]
    [StringLength(5000, MinimumLength = 10, ErrorMessage = "Açıklama en az 10, en fazla 5000 karakter olmalıdır.")]
    public string Description { get; set; } = string.Empty;

    public Guid? OrderId { get; set; }

    public Guid? ProductId { get; set; }
}
