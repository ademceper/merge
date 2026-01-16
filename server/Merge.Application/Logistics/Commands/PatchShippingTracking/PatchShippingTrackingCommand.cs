using MediatR;
using Merge.Application.DTOs.Logistics;

namespace Merge.Application.Logistics.Commands.PatchShippingTracking;

/// <summary>
/// PATCH command for partial shipping tracking updates
/// HIGH-API-001: PATCH Support - Partial updates without requiring all fields
/// </summary>
public record PatchShippingTrackingCommand(
    Guid ShippingId,
    PatchShippingTrackingDto PatchDto
) : IRequest<ShippingDto>;
