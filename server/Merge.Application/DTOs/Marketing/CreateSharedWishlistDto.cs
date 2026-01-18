using System.ComponentModel.DataAnnotations;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.DTOs.Marketing;


public record CreateSharedWishlistDto
{
    [Required]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "İsim en az 2, en fazla 200 karakter olmalıdır.")]
    public string Name { get; init; } = string.Empty;
    
    [StringLength(2000)]
    public string Description { get; init; } = string.Empty;
    
    public bool IsPublic { get; init; } = false;
    
    public List<Guid> ProductIds { get; init; } = new();
}
