using MediatR;
using Merge.Domain.Modules.Notifications;

namespace Merge.Application.Notification.Commands.DeleteTemplate;


public record DeleteTemplateCommand(Guid Id) : IRequest<bool>;
