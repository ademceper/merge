using MediatR;

namespace Merge.Application.Marketing.Commands.DeleteEmailAutomation;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record DeleteEmailAutomationCommand(Guid Id) : IRequest<bool>;
