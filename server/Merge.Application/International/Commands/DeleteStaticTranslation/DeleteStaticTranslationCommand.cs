using MediatR;

namespace Merge.Application.International.Commands.DeleteStaticTranslation;

public record DeleteStaticTranslationCommand(Guid Id) : IRequest<Unit>;

