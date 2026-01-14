using MediatR;

namespace Merge.Application.Marketing.Commands.DeleteEmailTemplate;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record DeleteEmailTemplateCommand(Guid Id) : IRequest<bool>;
