using System.ComponentModel.DataAnnotations;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.ValueObjects;

namespace Merge.Application.DTOs.Review;

public class UpdateReviewDto
{
    [Range(1, 5, ErrorMessage = "Puan 1 ile 5 arasında olmalıdır.")]
    public int Rating { get; set; }

    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [StringLength(2000, MinimumLength = 10, ErrorMessage = "Yorum en az 10, en fazla 2000 karakter olmalıdır.")]
    public string Comment { get; set; } = string.Empty;
}

