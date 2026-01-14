using System.ComponentModel.DataAnnotations;
using Merge.Domain.ValueObjects;

namespace Merge.Application.DTOs.Content;

[Obsolete("Use CreatePageBuilderCommand via MediatR instead")]
public class CreatePageBuilderDto
{
    [Required]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "İsim en az 2, en fazla 200 karakter olmalıdır.")]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Slug en az 2, en fazla 200 karakter olmalıdır.")]
    public string Slug { get; set; } = string.Empty;
    
    [Required]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Başlık en az 2, en fazla 200 karakter olmalıdır.")]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    [StringLength(50000, MinimumLength = 10, ErrorMessage = "İçerik en az 10, en fazla 50000 karakter olmalıdır.")]
    public string Content { get; set; } = string.Empty;
    
    [StringLength(100)]
    public string? Template { get; set; }
    
    public Guid? AuthorId { get; set; }
    
    [StringLength(50)]
    public string? PageType { get; set; }
    
    public Guid? RelatedEntityId { get; set; }
    
    [StringLength(200)]
    public string? MetaTitle { get; set; }
    
    [StringLength(500)]
    public string? MetaDescription { get; set; }
    
    [StringLength(500)]
    [Url(ErrorMessage = "Geçerli bir URL giriniz.")]
    public string? OgImageUrl { get; set; }
}

