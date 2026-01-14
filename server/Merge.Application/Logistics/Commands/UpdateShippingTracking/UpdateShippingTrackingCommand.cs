using MediatR;
using Merge.Application.DTOs.Logistics;

namespace Merge.Application.Logistics.Commands.UpdateShippingTracking;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record UpdateShippingTrackingCommand(
    Guid ShippingId,
    string TrackingNumber) : IRequest<ShippingDto>;

