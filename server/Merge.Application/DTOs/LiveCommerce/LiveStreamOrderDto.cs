namespace Merge.Application.DTOs.LiveCommerce;

public record LiveStreamOrderDto(
    Guid Id,
    Guid LiveStreamId,
    Guid OrderId,
    Guid? ProductId,
    decimal OrderAmount,
    DateTime CreatedAt);
