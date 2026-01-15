using MediatR;

namespace Merge.Application.International.Commands.DeleteCategoryTranslation;

public record DeleteCategoryTranslationCommand(Guid Id) : IRequest<Unit>;

