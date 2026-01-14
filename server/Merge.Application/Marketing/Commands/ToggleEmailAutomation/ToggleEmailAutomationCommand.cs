using MediatR;

namespace Merge.Application.Marketing.Commands.ToggleEmailAutomation;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record ToggleEmailAutomationCommand(Guid Id, bool IsActive) : IRequest<bool>;
