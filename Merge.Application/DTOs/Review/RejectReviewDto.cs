using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Review;

public class RejectReviewDto
{
    [Required]
    [StringLength(500, MinimumLength = 5, ErrorMessage = "Red nedeni en az 5, en fazla 500 karakter olmalıdır.")]
    public string Reason { get; set; } = string.Empty;
}

