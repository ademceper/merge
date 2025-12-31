using System.ComponentModel.DataAnnotations;
using Merge.Domain.Entities;

namespace Merge.Application.DTOs.Seller;

public class RejectApplicationDto
{
    [Required]
    [StringLength(1000, MinimumLength = 5, ErrorMessage = "Red nedeni en az 5, en fazla 1000 karakter olmalıdır.")]
    public string Reason { get; set; } = string.Empty;
}
