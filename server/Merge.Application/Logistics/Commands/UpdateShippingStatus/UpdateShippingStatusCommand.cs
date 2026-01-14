using MediatR;
using Merge.Application.DTOs.Logistics;
using Merge.Domain.Enums;

namespace Merge.Application.Logistics.Commands.UpdateShippingStatus;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.2: Enum kullanımı (string Status YASAK)
public record UpdateShippingStatusCommand(
    Guid ShippingId,
    ShippingStatus Status) : IRequest<ShippingDto>;

