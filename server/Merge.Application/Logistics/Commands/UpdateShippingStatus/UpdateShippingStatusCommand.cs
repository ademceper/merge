using MediatR;
using Merge.Application.DTOs.Logistics;
using Merge.Domain.Enums;

namespace Merge.Application.Logistics.Commands.UpdateShippingStatus;

public record UpdateShippingStatusCommand(
    Guid ShippingId,
    ShippingStatus Status) : IRequest<ShippingDto>;

