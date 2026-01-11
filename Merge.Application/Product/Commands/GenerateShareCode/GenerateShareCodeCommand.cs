using MediatR;

namespace Merge.Application.Product.Commands.GenerateShareCode;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GenerateShareCodeCommand(
    Guid ComparisonId
) : IRequest<string>;
