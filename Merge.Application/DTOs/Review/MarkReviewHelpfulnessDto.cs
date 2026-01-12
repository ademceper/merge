using System.ComponentModel.DataAnnotations;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.DTOs.Review;

public class MarkReviewHelpfulnessDto
{
    [Required]
    public Guid ReviewId { get; set; }
    
    public bool IsHelpful { get; set; }
}
