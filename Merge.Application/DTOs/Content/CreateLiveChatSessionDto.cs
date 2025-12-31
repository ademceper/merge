using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Content;

public class CreateLiveChatSessionDto
{
    [StringLength(100)]
    public string? GuestName { get; set; }
    
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
    [StringLength(200)]
    public string? GuestEmail { get; set; }
    
    [StringLength(50)]
    public string? Department { get; set; }
}
