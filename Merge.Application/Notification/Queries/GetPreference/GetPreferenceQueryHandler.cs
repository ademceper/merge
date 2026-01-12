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

/// <summary>
/// Get Preference Query Handler - BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class GetPreferenceQueryHandler : IRequestHandler<GetPreferenceQuery, NotificationPreferenceDto?>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;

    public GetPreferenceQueryHandler(IDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<NotificationPreferenceDto?> Handle(GetPreferenceQuery request, CancellationToken cancellationToken)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !np.IsDeleted (Global Query Filter)
        var preference = await _context.Set<NotificationPreference>()
            .AsNoTracking()
            .FirstOrDefaultAsync(np => np.UserId == request.UserId && 
                                      np.NotificationType == request.NotificationType && 
                                      np.Channel == request.Channel, cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return preference != null ? _mapper.Map<NotificationPreferenceDto>(preference) : null;
    }
}
