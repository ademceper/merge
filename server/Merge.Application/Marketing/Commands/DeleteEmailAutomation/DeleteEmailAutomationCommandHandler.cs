using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Notifications;
using Merge.Domain.ValueObjects;
using EmailAutomation = Merge.Domain.Modules.Notifications.EmailAutomation;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Marketing.Commands.DeleteEmailAutomation;

public class DeleteEmailAutomationCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    ILogger<DeleteEmailAutomationCommandHandler> logger) : IRequestHandler<DeleteEmailAutomationCommand, bool>
{
    public async Task<bool> Handle(DeleteEmailAutomationCommand request, CancellationToken cancellationToken)
    {
        var automation = await context.Set<EmailAutomation>()
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

        if (automation == null)
        {
            return false;
        }

        automation.MarkAsDeleted();
        
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Email otomasyonu silindi. AutomationId: {AutomationId}",
            request.Id);

        return true;
    }
}
