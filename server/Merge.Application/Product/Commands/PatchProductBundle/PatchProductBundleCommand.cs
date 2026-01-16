using MediatR;
using Merge.Application.DTOs.Product;

namespace Merge.Application.Product.Commands.PatchProductBundle;

/// <summary>
/// PATCH command for partial product bundle updates
/// HIGH-API-001: PATCH Support - Partial updates without requiring all fields
/// </summary>
public record PatchProductBundleCommand(
    Guid Id,
    PatchProductBundleDto PatchDto
) : IRequest<ProductBundleDto>;
