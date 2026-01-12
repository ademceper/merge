using Merge.Domain.Modules.Payment;
namespace Merge.Application.DTOs.B2B;

/// <summary>
/// WholesalePrice Response DTO - BOLUM 3.1: ProducesResponseType (ZORUNLU)
/// </summary>
public class WholesalePriceResponseDto
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public Guid? OrganizationId { get; set; }
    public decimal? Price { get; set; }
}

