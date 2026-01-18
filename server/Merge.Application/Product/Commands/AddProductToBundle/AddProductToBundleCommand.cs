using MediatR;
using Merge.Application.DTOs.Product;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Product.Commands.AddProductToBundle;

public record AddProductToBundleCommand(
    Guid BundleId,
    Guid ProductId,
    int Quantity,
    int SortOrder
) : IRequest<bool>;
