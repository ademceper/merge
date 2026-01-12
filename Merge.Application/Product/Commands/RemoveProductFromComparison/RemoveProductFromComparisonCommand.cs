using MediatR;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Product.Commands.RemoveProductFromComparison;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record RemoveProductFromComparisonCommand(
    Guid UserId,
    Guid ProductId
) : IRequest<bool>;
