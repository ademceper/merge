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

namespace Merge.Application.Marketing.Commands.ToggleEmailAutomation;

public class ToggleEmailAutomationCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    ILogger<ToggleEmailAutomationCommandHandler> logger) : IRequestHandler<ToggleEmailAutomationCommand, bool>
{
    public async Task<bool> Handle(ToggleEmailAutomationCommand request, CancellationToken cancellationToken)
    {
        var automation = await context.Set<EmailAutomation>()
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

        if (automation == null)
        {
            return false;
        }

        if (request.IsActive)
            automation.Activate();
        else
            automation.Deactivate();
        
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Email otomasyonu durumu değiştirildi. AutomationId: {AutomationId}, IsActive: {IsActive}",
            request.Id, request.IsActive);

        return true;
    }
}
