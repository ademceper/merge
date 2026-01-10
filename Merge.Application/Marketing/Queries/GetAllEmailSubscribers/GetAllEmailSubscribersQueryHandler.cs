using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Merge.Application.Common;
using Merge.Application.DTOs.Marketing;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;

namespace Merge.Application.Marketing.Queries.GetAllEmailSubscribers;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 3.4: Pagination (ZORUNLU)
public class GetAllEmailSubscribersQueryHandler : IRequestHandler<GetAllEmailSubscribersQuery, PagedResult<EmailSubscriberDto>>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;

    public GetAllEmailSubscribersQueryHandler(IDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PagedResult<EmailSubscriberDto>> Handle(GetAllEmailSubscribersQuery request, CancellationToken cancellationToken)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !s.IsDeleted (Global Query Filter)
        IQueryable<EmailSubscriber> query = _context.Set<EmailSubscriber>()
            .AsNoTracking();

        if (request.IsSubscribed.HasValue)
        {
            query = query.Where(s => s.IsSubscribed == request.IsSubscribed.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var subscribers = await query
            .OrderByDescending(s => s.SubscribedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return new PagedResult<EmailSubscriberDto>
        {
            Items = _mapper.Map<List<EmailSubscriberDto>>(subscribers),
            TotalCount = totalCount,
            Page = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}
