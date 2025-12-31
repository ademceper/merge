using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Content;

public class UpdateSitemapEntryDto
{
    [StringLength(500)]
    [Url(ErrorMessage = "Geçerli bir URL giriniz.")]
    public string? Url { get; set; }
    
    [StringLength(50)]
    public string? ChangeFrequency { get; set; }
    
    [Range(0, 1, ErrorMessage = "Öncelik 0 ile 1 arasında olmalıdır.")]
    public decimal? Priority { get; set; }
}
