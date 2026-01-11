using MediatR;

namespace Merge.Application.Product.Commands.ClearComparison;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record ClearComparisonCommand(
    Guid UserId
) : IRequest<bool>;
