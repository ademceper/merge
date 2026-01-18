using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Seller;

public record CreateSellerInvoiceDto
{
    [Required]
    public Guid SellerId { get; init; }
    
    [Required]
    public DateTime PeriodStart { get; init; }
    
    [Required]
    public DateTime PeriodEnd { get; init; }
    
    [StringLength(2000)]
    public string? Notes { get; init; }
}
