namespace Merge.Application.DTOs.LiveCommerce;

// ✅ BOLUM 4.2: Record DTOs (ZORUNLU) - Immutability için record kullan
public record LiveStreamOrderDto(
    Guid Id,
    Guid LiveStreamId,
    Guid OrderId,
    Guid? ProductId,
    decimal OrderAmount,
    DateTime CreatedAt);
