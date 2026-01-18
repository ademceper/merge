using MediatR;
using Merge.Application.DTOs.Logistics;

namespace Merge.Application.Logistics.Commands.UpdateShippingTracking;

public record UpdateShippingTrackingCommand(
    Guid ShippingId,
    string TrackingNumber) : IRequest<ShippingDto>;

