using MediatR;
using Merge.Domain.Modules.Notifications;

namespace Merge.Application.Notification.Commands.DeleteTemplate;

/// <summary>
/// Delete Template Command - BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public record DeleteTemplateCommand(Guid Id) : IRequest<bool>;
