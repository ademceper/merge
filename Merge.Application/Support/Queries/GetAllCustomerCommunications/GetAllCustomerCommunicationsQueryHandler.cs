using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Microsoft.Extensions.Options;
using Merge.Application.DTOs.Support;
using Merge.Application.Common;
using Merge.Application.Interfaces;
using Merge.Application.Configuration;
using Merge.Domain.Entities;

namespace Merge.Application.Support.Queries.GetAllCustomerCommunications;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetAllCustomerCommunicationsQueryHandler : IRequestHandler<GetAllCustomerCommunicationsQuery, PagedResult<CustomerCommunicationDto>>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly SupportSettings _settings;

    public GetAllCustomerCommunicationsQueryHandler(
        IDbContext context,
        IMapper mapper,
        IOptions<SupportSettings> settings)
    {
        _context = context;
        _mapper = mapper;
        _settings = settings.Value;
    }

    public async Task<PagedResult<CustomerCommunicationDto>> Handle(GetAllCustomerCommunicationsQuery request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 3.4: Pagination limit kontrolü (ZORUNLU)
        // ✅ BOLUM 12.0: Magic Number'ları Configuration'a Taşıma
        var pageSize = request.PageSize > 0 && request.PageSize <= _settings.MaxPageSize 
            ? request.PageSize 
            : _settings.DefaultPageSize;
        var page = request.Page > 0 ? request.Page : 1;

        // ✅ PERFORMANCE: AsNoTracking for read-only query, Global Query Filter otomatik uygulanır
        // ✅ PERFORMANCE: AsSplitQuery - Multiple Include'lar için query splitting (Cartesian Explosion önleme)
        IQueryable<CustomerCommunication> query = _context.Set<CustomerCommunication>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(c => c.User)
            .Include(c => c.SentBy);

        if (!string.IsNullOrEmpty(request.CommunicationType))
        {
            query = query.Where(c => c.CommunicationType == request.CommunicationType);
        }

        if (!string.IsNullOrEmpty(request.Channel))
        {
            query = query.Where(c => c.Channel == request.Channel);
        }

        if (request.UserId.HasValue)
        {
            query = query.Where(c => c.UserId == request.UserId.Value);
        }

        if (request.StartDate.HasValue)
        {
            query = query.Where(c => c.CreatedAt >= request.StartDate.Value);
        }

        if (request.EndDate.HasValue)
        {
            query = query.Where(c => c.CreatedAt <= request.EndDate.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var communications = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan
        return new PagedResult<CustomerCommunicationDto>
        {
            Items = _mapper.Map<List<CustomerCommunicationDto>>(communications),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}
