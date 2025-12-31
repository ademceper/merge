namespace Merge.Application.DTOs.LiveCommerce;

public class LiveStreamOrderDto
{
    public Guid Id { get; set; }
    public Guid LiveStreamId { get; set; }
    public Guid OrderId { get; set; }
    public Guid? ProductId { get; set; }
    public decimal OrderAmount { get; set; }
    public DateTime CreatedAt { get; set; }
}
