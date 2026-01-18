using MediatR;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Product.Commands.RemoveSizeGuideFromProduct;

public record RemoveSizeGuideFromProductCommand(
    Guid ProductId
) : IRequest<bool>;
