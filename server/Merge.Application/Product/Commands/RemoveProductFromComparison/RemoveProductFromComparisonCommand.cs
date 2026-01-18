using MediatR;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Product.Commands.RemoveProductFromComparison;

public record RemoveProductFromComparisonCommand(
    Guid UserId,
    Guid ProductId
) : IRequest<bool>;
