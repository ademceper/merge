using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.B2B;

public class CreatePurchaseOrderDto
{
    [Required]
    public Guid OrganizationId { get; set; }
    
    [Required]
    [MinLength(1, ErrorMessage = "En az bir ürün seçilmelidir.")]
    public List<CreatePurchaseOrderItemDto> Items { get; set; } = new();
    
    [StringLength(2000)]
    public string? Notes { get; set; }
    
    public DateTime? ExpectedDeliveryDate { get; set; }
    
    public Guid? CreditTermId { get; set; }
}
