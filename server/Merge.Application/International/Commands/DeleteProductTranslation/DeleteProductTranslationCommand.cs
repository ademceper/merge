using MediatR;

namespace Merge.Application.International.Commands.DeleteProductTranslation;

public record DeleteProductTranslationCommand(Guid Id) : IRequest<Unit>;

