using MediatR;
using Merge.Application.DTOs.Logistics;

namespace Merge.Application.Logistics.Commands.PatchShippingStatus;

/// <summary>
/// PATCH command for partial shipping status updates
/// HIGH-API-001: PATCH Support - Partial updates without requiring all fields
/// </summary>
public record PatchShippingStatusCommand(
    Guid ShippingId,
    PatchShippingStatusDto PatchDto
) : IRequest<ShippingDto>;
