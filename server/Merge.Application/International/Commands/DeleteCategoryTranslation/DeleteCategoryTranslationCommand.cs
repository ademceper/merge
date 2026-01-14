using MediatR;

namespace Merge.Application.International.Commands.DeleteCategoryTranslation;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record DeleteCategoryTranslationCommand(Guid Id) : IRequest<Unit>;

