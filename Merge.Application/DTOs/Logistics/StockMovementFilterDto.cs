using Merge.Domain.Entities;
namespace Merge.Application.DTOs.Logistics;

public class StockMovementFilterDto
{
    public Guid? ProductId { get; set; }
    public Guid? WarehouseId { get; set; }
    public StockMovementType? MovementType { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
