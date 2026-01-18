using System.ComponentModel.DataAnnotations;
using Merge.Domain.ValueObjects;

namespace Merge.Application.DTOs.Marketing;


public record CreateEmailSubscriberDto
{
    [Required]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
    [StringLength(200)]
    public string Email { get; init; } = string.Empty;
    
    [StringLength(100)]
    public string? FirstName { get; init; }
    
    [StringLength(100)]
    public string? LastName { get; init; }
    
    [StringLength(100)]
    public string? Source { get; init; }
    
    public List<string>? Tags { get; init; }
    
    public Dictionary<string, string>? CustomFields { get; init; }
}
