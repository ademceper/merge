using MediatR;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Product.Commands.DeleteComparison;

public record DeleteComparisonCommand(
    Guid Id,
    Guid UserId
) : IRequest<bool>;
