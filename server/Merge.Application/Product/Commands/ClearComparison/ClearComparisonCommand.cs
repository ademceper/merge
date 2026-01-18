using MediatR;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Product.Commands.ClearComparison;

public record ClearComparisonCommand(
    Guid UserId
) : IRequest<bool>;
