using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Merge.Application.DTOs.Notification;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using NotificationEntity = Merge.Domain.Modules.Notifications.Notification;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Notifications;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Notification.Queries.GetNotificationById;

/// <summary>
/// Get Notification By Id Query Handler - BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class GetNotificationByIdQueryHandler : IRequestHandler<GetNotificationByIdQuery, NotificationDto?>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;

    public GetNotificationByIdQueryHandler(IDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<NotificationDto?> Handle(GetNotificationByIdQuery request, CancellationToken cancellationToken)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !n.IsDeleted (Global Query Filter)
        var notification = await _context.Set<NotificationEntity>()
            .AsNoTracking()
            .FirstOrDefaultAsync(n => n.Id == request.NotificationId, cancellationToken);

        if (notification == null)
        {
            return null;
        }

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<NotificationDto>(notification);
    }
}
