using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Content;

/// <summary>
/// Structured Data DTO - BOLUM 4.3: Over-Posting Koruması (Dictionary&lt;string, object&gt; YASAK)
/// JSON-LD structured data için typed DTO
/// </summary>
public class StructuredDataDto
{
    [StringLength(50)]
    public string? Type { get; set; } // @type for JSON-LD

    [StringLength(200)]
    public string? Name { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    [StringLength(500)]
    [Url(ErrorMessage = "Geçerli bir URL giriniz.")]
    public string? Image { get; set; }

    [StringLength(500)]
    [Url(ErrorMessage = "Geçerli bir URL giriniz.")]
    public string? Url { get; set; }

    // Additional properties can be added as needed
    // For complex structured data, consider using JsonElement or string for flexibility
    // but with proper validation
}

