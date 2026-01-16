using MediatR;
using Merge.Application.DTOs.Product;

namespace Merge.Application.Product.Commands.PatchProduct;

/// <summary>
/// PATCH command for partial product updates
/// HIGH-API-001: PATCH Support - Partial updates without requiring all fields
/// </summary>
public record PatchProductCommand(
    Guid Id,
    PatchProductDto PatchDto,
    Guid? PerformedBy = null // IDOR protection için
) : IRequest<ProductDto>;
