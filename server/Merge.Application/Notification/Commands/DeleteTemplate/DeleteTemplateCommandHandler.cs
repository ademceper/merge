using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Notifications;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Notification.Commands.DeleteTemplate;


public class DeleteTemplateCommandHandler(IDbContext context, IUnitOfWork unitOfWork, ILogger<DeleteTemplateCommandHandler> logger) : IRequestHandler<DeleteTemplateCommand, bool>
{

    public async Task<bool> Handle(DeleteTemplateCommand request, CancellationToken cancellationToken)
    {
        var template = await context.Set<NotificationTemplate>()
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

        if (template == null)
        {
            return false;
        }

        template.Delete();

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Notification template silindi. TemplateId: {TemplateId}",
            request.Id);

        return true;
    }
}
