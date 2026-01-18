using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Notifications;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Notification.Commands.DeletePreference;


public class DeletePreferenceCommandHandler(IDbContext context, IUnitOfWork unitOfWork, ILogger<DeletePreferenceCommandHandler> logger) : IRequestHandler<DeletePreferenceCommand, bool>
{

    public async Task<bool> Handle(DeletePreferenceCommand request, CancellationToken cancellationToken)
    {
        var preference = await context.Set<NotificationPreference>()
            .FirstOrDefaultAsync(np => np.UserId == request.UserId && 
                                      np.NotificationType == request.NotificationType && 
                                      np.Channel == request.Channel, cancellationToken);

        if (preference == null)
        {
            return false;
        }

        preference.Delete();

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Notification preference silindi. UserId: {UserId}, NotificationType: {NotificationType}, Channel: {Channel}",
            request.UserId, request.NotificationType, request.Channel);

        return true;
    }
}
