using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Marketing;

public class CreateEmailTemplateDto
{
    [Required]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Şablon adı en az 2, en fazla 200 karakter olmalıdır.")]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;
    
    [Required]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Konu en az 2, en fazla 200 karakter olmalıdır.")]
    public string Subject { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100000, MinimumLength = 10, ErrorMessage = "HTML içerik en az 10, en fazla 100000 karakter olmalıdır.")]
    public string HtmlContent { get; set; } = string.Empty;
    
    [StringLength(50000)]
    public string TextContent { get; set; } = string.Empty;
    
    [StringLength(50)]
    public string Type { get; set; } = "Custom";
    
    public List<string>? Variables { get; set; }
}
