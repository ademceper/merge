using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Marketing;

public class CreateEmailSubscriberDto
{
    [Required]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
    [StringLength(200)]
    public string Email { get; set; } = string.Empty;
    
    [StringLength(100)]
    public string? FirstName { get; set; }
    
    [StringLength(100)]
    public string? LastName { get; set; }
    
    [StringLength(100)]
    public string? Source { get; set; }
    
    public List<string>? Tags { get; set; }
    
    public Dictionary<string, string>? CustomFields { get; set; }
}
