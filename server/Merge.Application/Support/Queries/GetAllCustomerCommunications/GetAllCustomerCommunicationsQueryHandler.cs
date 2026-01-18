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

namespace Merge.Application.Support.Queries.GetAllCustomerCommunications;

public class GetAllCustomerCommunicationsQueryHandler(IDbContext context, IMapper mapper, IOptions<SupportSettings> settings) : IRequestHandler<GetAllCustomerCommunicationsQuery, PagedResult<CustomerCommunicationDto>>
{
    private readonly SupportSettings supportConfig = settings.Value;

    public async Task<PagedResult<CustomerCommunicationDto>> Handle(GetAllCustomerCommunicationsQuery request, CancellationToken cancellationToken)
    {
        var pageSize = request.PageSize > 0 && request.PageSize <= supportConfig.MaxPageSize 
            ? request.PageSize 
            : supportConfig.DefaultPageSize;
        var page = request.Page > 0 ? request.Page : 1;

        IQueryable<CustomerCommunication> query = context.Set<CustomerCommunication>()
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

        return new PagedResult<CustomerCommunicationDto>
        {
            Items = mapper.Map<List<CustomerCommunicationDto>>(communications),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}
