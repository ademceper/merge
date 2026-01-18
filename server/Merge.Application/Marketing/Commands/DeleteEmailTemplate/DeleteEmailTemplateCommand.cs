using MediatR;

namespace Merge.Application.Marketing.Commands.DeleteEmailTemplate;

public record DeleteEmailTemplateCommand(Guid Id) : IRequest<bool>;
