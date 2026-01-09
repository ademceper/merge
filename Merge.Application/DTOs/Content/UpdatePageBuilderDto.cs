using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Content;

[Obsolete("Use UpdatePageBuilderCommand via MediatR instead")]
public class UpdatePageBuilderDto
{
    [StringLength(200, MinimumLength = 2, ErrorMessage = "İsim en az 2, en fazla 200 karakter olmalıdır.")]
    public string? Name { get; set; }
    
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Slug en az 2, en fazla 200 karakter olmalıdır.")]
    public string? Slug { get; set; }
    
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Başlık en az 2, en fazla 200 karakter olmalıdır.")]
    public string? Title { get; set; }
    
    [StringLength(50000)]
    public string? Content { get; set; }
    
    [StringLength(100)]
    public string? Template { get; set; }
    
    [StringLength(200)]
    public string? MetaTitle { get; set; }
    
    [StringLength(500)]
    public string? MetaDescription { get; set; }
    
    [StringLength(500)]
    [Url(ErrorMessage = "Geçerli bir URL giriniz.")]
    public string? OgImageUrl { get; set; }
}

