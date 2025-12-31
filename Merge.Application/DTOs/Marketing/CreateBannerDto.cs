using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Marketing;

public class CreateBannerDto
{
    [Required]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Başlık en az 2, en fazla 200 karakter olmalıdır.")]
    public string Title { get; set; } = string.Empty;
    
    [StringLength(1000)]
    public string? Description { get; set; }
    
    [Required]
    [StringLength(500)]
    [Url(ErrorMessage = "Geçerli bir URL giriniz.")]
    public string ImageUrl { get; set; } = string.Empty;
    
    [StringLength(500)]
    [Url(ErrorMessage = "Geçerli bir URL giriniz.")]
    public string? LinkUrl { get; set; }
    
    [StringLength(50)]
    public string Position { get; set; } = "Homepage";
    
    [Range(0, int.MaxValue)]
    public int SortOrder { get; set; } = 0;
    
    public DateTime? StartDate { get; set; }
    
    public DateTime? EndDate { get; set; }
    
    public Guid? CategoryId { get; set; }
    
    public Guid? ProductId { get; set; }
}
