using MediatR;

namespace Merge.Application.International.Commands.DeleteLanguage;

public record DeleteLanguageCommand(Guid Id) : IRequest<Unit>;

