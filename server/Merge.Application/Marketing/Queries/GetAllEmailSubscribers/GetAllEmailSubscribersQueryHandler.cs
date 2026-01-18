using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Merge.Application.Common;
using Merge.Application.DTOs.Marketing;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Marketing;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Marketing.Queries.GetAllEmailSubscribers;

public class GetAllEmailSubscribersQueryHandler(IDbContext context, IMapper mapper) : IRequestHandler<GetAllEmailSubscribersQuery, PagedResult<EmailSubscriberDto>>
{
    public async Task<PagedResult<EmailSubscriberDto>> Handle(GetAllEmailSubscribersQuery request, CancellationToken cancellationToken)
    {
        IQueryable<EmailSubscriber> query = context.Set<EmailSubscriber>()
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

        return new PagedResult<EmailSubscriberDto>
        {
            Items = mapper.Map<List<EmailSubscriberDto>>(subscribers),
            TotalCount = totalCount,
            Page = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}
