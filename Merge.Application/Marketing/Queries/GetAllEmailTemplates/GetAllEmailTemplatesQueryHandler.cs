using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Merge.Application.Common;
using Merge.Application.DTOs.Marketing;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Notifications;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Marketing.Queries.GetAllEmailTemplates;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 3.4: Pagination (ZORUNLU)
public class GetAllEmailTemplatesQueryHandler : IRequestHandler<GetAllEmailTemplatesQuery, PagedResult<EmailTemplateDto>>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;

    public GetAllEmailTemplatesQueryHandler(IDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PagedResult<EmailTemplateDto>> Handle(GetAllEmailTemplatesQuery request, CancellationToken cancellationToken)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !t.IsDeleted (Global Query Filter)
        IQueryable<EmailTemplate> query = _context.Set<EmailTemplate>()
            .AsNoTracking()
            .Where(t => t.IsActive);

        if (!string.IsNullOrEmpty(request.Type))
        {
            if (Enum.TryParse<EmailTemplateType>(request.Type, true, out var typeEnum))
            {
                query = query.Where(t => t.Type == typeEnum);
            }
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var templates = await query
            .OrderBy(t => t.Name)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<EmailTemplateDto>
        {
            Items = _mapper.Map<List<EmailTemplateDto>>(templates),
            TotalCount = totalCount,
            Page = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}
