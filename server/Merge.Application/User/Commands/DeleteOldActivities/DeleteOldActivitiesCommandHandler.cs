using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Merge.Application.Configuration;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Identity;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.User.Commands.DeleteOldActivities;

public class DeleteOldActivitiesCommandHandler(IDbContext context, IUnitOfWork unitOfWork, ILogger<DeleteOldActivitiesCommandHandler> logger, IOptions<UserSettings> userSettings) : IRequestHandler<DeleteOldActivitiesCommand>
{
    public async Task Handle(DeleteOldActivitiesCommand request, CancellationToken cancellationToken)
    {

        logger.LogInformation("Deleting old activities older than {Days} days", request.DaysToKeep);
        var daysToKeep = request.DaysToKeep;
        if (daysToKeep < userSettings.Value.Activity.MinDaysToKeep) daysToKeep = userSettings.Value.Activity.MinDaysToKeep;
        if (daysToKeep > userSettings.Value.Activity.MaxDaysToKeep) daysToKeep = userSettings.Value.Activity.MaxDaysToKeep;

        var cutoffDate = DateTime.UtcNow.AddDays(-daysToKeep);

        var oldActivities = await context.Set<UserActivityLog>()
            .Where(a => a.CreatedAt < cutoffDate)
            .ToListAsync(cancellationToken);

        context.Set<UserActivityLog>().RemoveRange(oldActivities);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        

        logger.LogWarning("Deleted {Count} old activity records", oldActivities.Count);
    }
}
