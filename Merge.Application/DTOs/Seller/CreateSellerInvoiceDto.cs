using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Seller;

public class CreateSellerInvoiceDto
{
    [Required]
    public Guid SellerId { get; set; }
    
    [Required]
    public DateTime PeriodStart { get; set; }
    
    [Required]
    public DateTime PeriodEnd { get; set; }
    
    [StringLength(2000)]
    public string? Notes { get; set; }
}
