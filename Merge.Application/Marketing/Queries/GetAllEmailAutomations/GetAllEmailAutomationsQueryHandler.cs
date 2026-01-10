using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Merge.Application.Common;
using Merge.Application.DTOs.Marketing;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;

namespace Merge.Application.Marketing.Queries.GetAllEmailAutomations;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 3.4: Pagination (ZORUNLU)
public class GetAllEmailAutomationsQueryHandler : IRequestHandler<GetAllEmailAutomationsQuery, PagedResult<EmailAutomationDto>>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;

    public GetAllEmailAutomationsQueryHandler(IDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PagedResult<EmailAutomationDto>> Handle(GetAllEmailAutomationsQuery request, CancellationToken cancellationToken)
    {
        // ✅ PERFORMANCE: AsSplitQuery - N+1 query önleme (Cartesian Explosion önleme)
        var query = _context.Set<EmailAutomation>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(a => a.Template)
            .OrderByDescending(a => a.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);
        var automations = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<EmailAutomationDto>
        {
            Items = _mapper.Map<List<EmailAutomationDto>>(automations),
            TotalCount = totalCount,
            Page = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}
