using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Merge.Application.DTOs.Notification;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;

namespace Merge.Application.Notification.Queries.GetTemplateByType;

/// <summary>
/// Get Template By Type Query Handler - BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class GetTemplateByTypeQueryHandler : IRequestHandler<GetTemplateByTypeQuery, NotificationTemplateDto?>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;

    public GetTemplateByTypeQueryHandler(IDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<NotificationTemplateDto?> Handle(GetTemplateByTypeQuery request, CancellationToken cancellationToken)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !t.IsDeleted (Global Query Filter)
        var template = await _context.Set<NotificationTemplate>()
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Type == request.Type && t.IsActive, cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return template != null ? _mapper.Map<NotificationTemplateDto>(template) : null;
    }
}
