using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Review;

public class MarkReviewHelpfulnessDto
{
    [Required]
    public Guid ReviewId { get; set; }
    
    public bool IsHelpful { get; set; }
}
