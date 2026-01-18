using MediatR;

namespace Merge.Application.Marketing.Commands.DeleteEmailAutomation;

public record DeleteEmailAutomationCommand(Guid Id) : IRequest<bool>;
