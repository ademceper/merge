using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Merge.Application.Common;
using Merge.Application.DTOs.B2B;
using Merge.Application.Interfaces;
using Merge.Application.Configuration;
using Merge.Domain.Entities;
using AutoMapper;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Payment;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.B2B.Queries.GetOrganizationCreditTerms;

public class GetOrganizationCreditTermsQueryHandler(
    IDbContext context,
    IMapper mapper,
    ILogger<GetOrganizationCreditTermsQueryHandler> logger,
    IOptions<PaginationSettings> paginationSettings) : IRequestHandler<GetOrganizationCreditTermsQuery, PagedResult<CreditTermDto>>
{
    private readonly PaginationSettings paginationConfig = paginationSettings.Value;

    public async Task<PagedResult<CreditTermDto>> Handle(GetOrganizationCreditTermsQuery request, CancellationToken cancellationToken)
    {
        var pageSize = request.PageSize > paginationConfig.MaxPageSize ? paginationConfig.MaxPageSize : request.PageSize;
        var page = request.Page < 1 ? 1 : request.Page;

        var query = context.Set<CreditTerm>()
            .AsNoTracking()
            .Include(ct => ct.Organization)
            .Where(ct => ct.OrganizationId == request.OrganizationId);

        if (request.IsActive.HasValue)
        {
            query = query.Where(ct => ct.IsActive == request.IsActive.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var creditTerms = await query
            .OrderBy(ct => ct.PaymentDays)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var items = mapper.Map<List<CreditTermDto>>(creditTerms);

        return new PagedResult<CreditTermDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}

