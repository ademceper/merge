using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Marketing;

public class CreateSharedWishlistDto
{
    [Required]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "İsim en az 2, en fazla 200 karakter olmalıdır.")]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(2000)]
    public string Description { get; set; } = string.Empty;
    
    public bool IsPublic { get; set; } = false;
    
    public List<Guid> ProductIds { get; set; } = new();
}
