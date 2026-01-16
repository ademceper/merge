using MediatR;
using Merge.Application.DTOs.Logistics;

namespace Merge.Application.Logistics.Commands.PatchWarehouse;

/// <summary>
/// PATCH command for partial warehouse updates
/// HIGH-API-001: PATCH Support - Partial updates without requiring all fields
/// </summary>
public record PatchWarehouseCommand(
    Guid Id,
    PatchWarehouseDto PatchDto
) : IRequest<WarehouseDto>;
