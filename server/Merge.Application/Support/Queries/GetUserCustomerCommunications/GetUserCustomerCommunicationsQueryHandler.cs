using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Microsoft.Extensions.Options;
using Merge.Application.DTOs.Support;
using Merge.Application.Common;
using Merge.Application.Interfaces;
using Merge.Application.Configuration;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Support;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Support.Queries.GetUserCustomerCommunications;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetUserCustomerCommunicationsQueryHandler(IDbContext context, IMapper mapper, IOptions<PaginationSettings> paginationSettings) : IRequestHandler<GetUserCustomerCommunicationsQuery, PagedResult<CustomerCommunicationDto>>
{
    private readonly PaginationSettings paginationConfig = paginationSettings.Value;

    public async Task<PagedResult<CustomerCommunicationDto>> Handle(GetUserCustomerCommunicationsQuery request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 3.4: Pagination limit kontrolü (ZORUNLU)
        // ✅ BOLUM 12.0: Magic Number'ları Configuration'a Taşıma
        var pageSize = request.PageSize > 0 && request.PageSize <= paginationConfig.MaxPageSize 
            ? request.PageSize 
            : paginationConfig.DefaultPageSize;
        var page = request.Page > 0 ? request.Page : 1;

        // ✅ PERFORMANCE: AsNoTracking for read-only query, Global Query Filter otomatik uygulanır
        // ✅ PERFORMANCE: AsSplitQuery - Multiple Include'lar için query splitting (Cartesian Explosion önleme)
        IQueryable<CustomerCommunication> query = context.Set<CustomerCommunication>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(c => c.User)
            .Include(c => c.SentBy)
            .Where(c => c.UserId == request.UserId);

        if (!string.IsNullOrEmpty(request.CommunicationType))
        {
            query = query.Where(c => c.CommunicationType == request.CommunicationType);
        }

        if (!string.IsNullOrEmpty(request.Channel))
        {
            query = query.Where(c => c.Channel == request.Channel);
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
            Items = mapper.Map<List<CustomerCommunicationDto>>(communications),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}
