using MediatR;

namespace Merge.Application.Support.Commands.IncrementFaqViewCount;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record IncrementFaqViewCountCommand(
    Guid FaqId
) : IRequest<bool>;
