using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using AutoMapper;
using Merge.Application.Common;
using Merge.Application.Configuration;
using Merge.Application.DTOs.Notification;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Notifications;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Notification.Queries.GetTemplates;

/// <summary>
/// Get Templates Query Handler - BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// BOLUM 3.4: Pagination (ZORUNLU)
/// </summary>
public class GetTemplatesQueryHandler : IRequestHandler<GetTemplatesQuery, PagedResult<NotificationTemplateDto>>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly PaginationSettings _paginationSettings;

    public GetTemplatesQueryHandler(
        IDbContext context,
        IMapper mapper,
        IOptions<PaginationSettings> paginationSettings)
    {
        _context = context;
        _mapper = mapper;
        _paginationSettings = paginationSettings.Value;
    }

    public async Task<PagedResult<NotificationTemplateDto>> Handle(GetTemplatesQuery request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 12.0: Magic Numbers YASAK - Configuration kullan
        var pageSize = request.PageSize > _paginationSettings.MaxPageSize 
            ? _paginationSettings.MaxPageSize 
            : request.PageSize;
        var page = request.Page < 1 ? 1 : request.Page;

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !t.IsDeleted (Global Query Filter)
        IQueryable<NotificationTemplate> query = _context.Set<NotificationTemplate>()
            .AsNoTracking();

        if (request.Type.HasValue)
        {
            query = query.Where(t => t.Type == request.Type.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var templates = await query
            .OrderBy(t => t.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        var templateDtos = _mapper.Map<List<NotificationTemplateDto>>(templates);

        return new PagedResult<NotificationTemplateDto>
        {
            Items = templateDtos,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}
