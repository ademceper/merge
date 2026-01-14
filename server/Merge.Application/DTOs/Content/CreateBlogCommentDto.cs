using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Content;

[Obsolete("Use CreateBlogCommentCommand via MediatR instead")]
public class CreateBlogCommentDto
{
    [Required]
    public Guid BlogPostId { get; set; }
    
    public Guid? ParentCommentId { get; set; }
    
    [StringLength(100)]
    public string? AuthorName { get; set; } // For guest comments
    
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
    [StringLength(200)]
    public string? AuthorEmail { get; set; } // For guest comments
    
    [Required]
    [StringLength(2000, MinimumLength = 1, ErrorMessage = "Yorum içeriği en az 1, en fazla 2000 karakter olmalıdır.")]
    public string Content { get; set; } = string.Empty;
}

