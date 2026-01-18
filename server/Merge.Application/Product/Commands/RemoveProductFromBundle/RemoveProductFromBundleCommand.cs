using MediatR;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Product.Commands.RemoveProductFromBundle;

public record RemoveProductFromBundleCommand(
    Guid BundleId,
    Guid ProductId
) : IRequest<bool>;
