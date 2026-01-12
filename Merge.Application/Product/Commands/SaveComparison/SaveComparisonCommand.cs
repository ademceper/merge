using MediatR;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Product.Commands.SaveComparison;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record SaveComparisonCommand(
    Guid UserId,
    string Name
) : IRequest<bool>;
