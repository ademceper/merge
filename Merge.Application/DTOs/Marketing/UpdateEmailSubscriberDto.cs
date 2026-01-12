using System.ComponentModel.DataAnnotations;
using Merge.Domain.ValueObjects;

namespace Merge.Application.DTOs.Marketing;

/// <summary>
/// Update Email Subscriber DTO - BOLUM 1.0: DTO Dosya Organizasyonu (ZORUNLU)
/// </summary>
public record UpdateEmailSubscriberDto
{
    [StringLength(100)]
    public string? FirstName { get; init; }
    
    [StringLength(100)]
    public string? LastName { get; init; }
    
    [StringLength(100)]
    public string? Source { get; init; }
    
    public List<string>? Tags { get; init; }
    
    public Dictionary<string, string>? CustomFields { get; init; }
    
    public bool? IsSubscribed { get; init; }
}
