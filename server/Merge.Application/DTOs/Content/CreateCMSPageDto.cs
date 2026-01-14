using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Content;

[Obsolete("Use CreateCMSPageCommand via MediatR instead")]
public class CreateCMSPageDto
{
    [Required]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Başlık en az 2, en fazla 200 karakter olmalıdır.")]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    [StringLength(10000, MinimumLength = 10, ErrorMessage = "İçerik en az 10, en fazla 10000 karakter olmalıdır.")]
    public string Content { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? Excerpt { get; set; }
    
    [StringLength(50)]
    public string PageType { get; set; } = "Page";
    
    [StringLength(50)]
    public string Status { get; set; } = "Draft";
    
    [StringLength(100)]
    public string? Template { get; set; }
    
    [StringLength(200)]
    public string? MetaTitle { get; set; }
    
    [StringLength(500)]
    public string? MetaDescription { get; set; }
    
    [StringLength(200)]
    public string? MetaKeywords { get; set; }
    
    public bool IsHomePage { get; set; } = false;
    
    [Range(0, int.MaxValue)]
    public int DisplayOrder { get; set; } = 0;
    
    public bool ShowInMenu { get; set; } = true;
    
    [StringLength(100)]
    public string? MenuTitle { get; set; }
    
    public Guid? ParentPageId { get; set; }
}

