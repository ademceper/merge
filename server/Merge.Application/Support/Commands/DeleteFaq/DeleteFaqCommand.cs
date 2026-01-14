using MediatR;

namespace Merge.Application.Support.Commands.DeleteFaq;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record DeleteFaqCommand(
    Guid FaqId
) : IRequest<bool>;
