using MediatR;

namespace Merge.Application.International.Commands.DeleteProductTranslation;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record DeleteProductTranslationCommand(Guid Id) : IRequest<Unit>;

