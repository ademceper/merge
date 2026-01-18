using MediatR;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Product.Commands.DeleteProductBundle;

public record DeleteProductBundleCommand(
    Guid Id
) : IRequest<bool>;
