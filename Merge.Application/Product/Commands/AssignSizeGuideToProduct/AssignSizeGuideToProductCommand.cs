using MediatR;
using Merge.Application.DTOs.Product;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Product.Commands.AssignSizeGuideToProduct;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record AssignSizeGuideToProductCommand(
    Guid ProductId,
    Guid SizeGuideId,
    string? CustomNotes,
    bool FitType,
    string? FitDescription
) : IRequest;
