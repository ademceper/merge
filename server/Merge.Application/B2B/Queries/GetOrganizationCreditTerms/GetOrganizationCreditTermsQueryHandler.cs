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

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class GetOrganizationCreditTermsQueryHandler : IRequestHandler<GetOrganizationCreditTermsQuery, PagedResult<CreditTermDto>>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetOrganizationCreditTermsQueryHandler> _logger;
    private readonly PaginationSettings _paginationSettings;

    public GetOrganizationCreditTermsQueryHandler(
        IDbContext context,
        IMapper mapper,
        ILogger<GetOrganizationCreditTermsQueryHandler> logger,
        IOptions<PaginationSettings> paginationSettings)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _paginationSettings = paginationSettings.Value;
    }

    public async Task<PagedResult<CreditTermDto>> Handle(GetOrganizationCreditTermsQuery request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 3.4: Pagination limit kontrolü (ZORUNLU)
        var pageSize = request.PageSize > _paginationSettings.MaxPageSize ? _paginationSettings.MaxPageSize : request.PageSize;
        var page = request.Page < 1 ? 1 : request.Page;

        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        // ✅ PERFORMANCE: Removed manual !ct.IsDeleted check (Global Query Filter handles it)
        var query = _context.Set<CreditTerm>()
            .AsNoTracking()
            .Include(ct => ct.Organization)
            .Where(ct => ct.OrganizationId == request.OrganizationId);

        if (request.IsActive.HasValue)
        {
            query = query.Where(ct => ct.IsActive == request.IsActive.Value);
        }

        // ✅ PERFORMANCE: TotalCount için ayrı query (CountAsync)
        var totalCount = await query.CountAsync(cancellationToken);

        var creditTerms = await query
            .OrderBy(ct => ct.PaymentDays)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullanımı (manuel mapping yerine)
        var items = _mapper.Map<List<CreditTermDto>>(creditTerms);

        // ✅ BOLUM 3.4: Pagination (ZORUNLU) - PagedResult döndürüyor
        return new PagedResult<CreditTermDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}

