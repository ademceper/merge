using System.ComponentModel.DataAnnotations;
using Merge.Domain.Entities;
using Merge.Domain.Enums;

namespace Merge.Application.DTOs.Seller;

public class ReviewSellerApplicationDto
{
    [Required]
    public SellerApplicationStatus Status { get; set; }
    
    [StringLength(1000)]
    public string? RejectionReason { get; set; }
    
    [StringLength(2000)]
    public string? AdditionalNotes { get; set; }
}
