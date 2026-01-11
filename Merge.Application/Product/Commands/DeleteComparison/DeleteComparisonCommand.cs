using MediatR;

namespace Merge.Application.Product.Commands.DeleteComparison;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record DeleteComparisonCommand(
    Guid Id,
    Guid UserId
) : IRequest<bool>;
