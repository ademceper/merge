using MediatR;
using Merge.Application.DTOs.Product;

namespace Merge.Application.Product.Commands.PatchSizeGuide;

/// <summary>
/// PATCH command for partial size guide updates
/// HIGH-API-001: PATCH Support - Partial updates without requiring all fields
/// </summary>
public record PatchSizeGuideCommand(
    Guid Id,
    PatchSizeGuideDto PatchDto
) : IRequest<bool>;
