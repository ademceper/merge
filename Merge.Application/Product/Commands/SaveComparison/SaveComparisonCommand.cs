using MediatR;

namespace Merge.Application.Product.Commands.SaveComparison;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record SaveComparisonCommand(
    Guid UserId,
    string Name
) : IRequest<bool>;
