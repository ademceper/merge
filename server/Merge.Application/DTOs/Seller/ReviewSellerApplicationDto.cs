using System.ComponentModel.DataAnnotations;
using Merge.Domain.Entities;
using Merge.Domain.Enums;

namespace Merge.Application.DTOs.Seller;

// ✅ BOLUM 7.1.5: Records - DTO'lar record olarak tanımlanmalı (ZORUNLU)
// ✅ BOLUM 8.0: Over-posting Protection - init-only properties (ZORUNLU)
public record ReviewSellerApplicationDto
{
    [Required]
    public SellerApplicationStatus Status { get; init; }
    
    [StringLength(1000)]
    public string? RejectionReason { get; init; }
    
    [StringLength(2000)]
    public string? AdditionalNotes { get; init; }
}
