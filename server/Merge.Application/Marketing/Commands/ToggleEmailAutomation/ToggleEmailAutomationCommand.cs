using MediatR;

namespace Merge.Application.Marketing.Commands.ToggleEmailAutomation;

public record ToggleEmailAutomationCommand(Guid Id, bool IsActive) : IRequest<bool>;
