using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Merge.Application.DTOs.Notification;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Notifications;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Notification.Queries.GetTemplateByType;


public class GetTemplateByTypeQueryHandler(IDbContext context, IMapper mapper) : IRequestHandler<GetTemplateByTypeQuery, NotificationTemplateDto?>
{

    public async Task<NotificationTemplateDto?> Handle(GetTemplateByTypeQuery request, CancellationToken cancellationToken)
    {
        var template = await context.Set<NotificationTemplate>()
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Type == request.Type && t.IsActive, cancellationToken);

        return template != null ? mapper.Map<NotificationTemplateDto>(template) : null;
    }
}
