using MediatR;

namespace Merge.Application.International.Commands.DeleteLanguage;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record DeleteLanguageCommand(Guid Id) : IRequest<Unit>;

