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

namespace Merge.Application.Notification.Queries.GetTemplate;


public class GetTemplateQueryHandler(IDbContext context, IMapper mapper) : IRequestHandler<GetTemplateQuery, NotificationTemplateDto?>
{

    public async Task<NotificationTemplateDto?> Handle(GetTemplateQuery request, CancellationToken cancellationToken)
    {
        var template = await context.Set<NotificationTemplate>()
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

        return template is not null ? mapper.Map<NotificationTemplateDto>(template) : null;
    }
}
