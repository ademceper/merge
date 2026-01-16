using MediatR;
using Merge.Application.DTOs.Catalog;
using Merge.Application.DTOs.Logistics;

namespace Merge.Application.Catalog.Commands.PatchInventory;

/// <summary>
/// PATCH command for partial inventory updates
/// HIGH-API-001: PATCH Support - Partial updates without requiring all fields
/// </summary>
public record PatchInventoryCommand(
    Guid Id,
    PatchInventoryDto PatchDto,
    Guid PerformedBy
) : IRequest<InventoryDto>;
