using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Merge.Application.DTOs.Notification;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Notifications;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Notification.Queries.GetPreference;


public class GetPreferenceQueryHandler(IDbContext context, IMapper mapper) : IRequestHandler<GetPreferenceQuery, NotificationPreferenceDto?>
{

    public async Task<NotificationPreferenceDto?> Handle(GetPreferenceQuery request, CancellationToken cancellationToken)
    {
        var preference = await context.Set<NotificationPreference>()
            .AsNoTracking()
            .FirstOrDefaultAsync(np => np.UserId == request.UserId && 
                                      np.NotificationType == request.NotificationType && 
                                      np.Channel == request.Channel, cancellationToken);

        return preference is not null ? mapper.Map<NotificationPreferenceDto>(preference) : null;
    }
}
