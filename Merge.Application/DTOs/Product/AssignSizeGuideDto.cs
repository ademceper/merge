using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Product;

public class AssignSizeGuideDto
{
    [Required]
    public Guid ProductId { get; set; }
    
    [Required]
    public Guid SizeGuideId { get; set; }
    
    [StringLength(1000)]
    public string? CustomNotes { get; set; }
    
    [StringLength(2000)]
    public string? FitDescription { get; set; }
}
