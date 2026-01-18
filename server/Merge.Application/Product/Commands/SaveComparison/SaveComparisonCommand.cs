using MediatR;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Product.Commands.SaveComparison;

public record SaveComparisonCommand(
    Guid UserId,
    string Name
) : IRequest<bool>;
