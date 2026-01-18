using MediatR;
using Merge.Application.DTOs.Product;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Product.Commands.AssignSizeGuideToProduct;

public record AssignSizeGuideToProductCommand(
    Guid ProductId,
    Guid SizeGuideId,
    string? CustomNotes,
    bool FitType,
    string? FitDescription
) : IRequest;
