using MediatR;

namespace Merge.Application.International.Commands.DeleteStaticTranslation;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record DeleteStaticTranslationCommand(Guid Id) : IRequest<Unit>;

