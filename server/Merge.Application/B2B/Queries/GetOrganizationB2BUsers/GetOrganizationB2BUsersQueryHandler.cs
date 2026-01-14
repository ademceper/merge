using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Merge.Application.Common;
using Merge.Application.DTOs.B2B;
using Merge.Application.Interfaces;
using Merge.Application.Configuration;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using AutoMapper;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Identity;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.B2B.Queries.GetOrganizationB2BUsers;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class GetOrganizationB2BUsersQueryHandler(
    IDbContext context,
    IMapper mapper,
    ILogger<GetOrganizationB2BUsersQueryHandler> logger,
    IOptions<PaginationSettings> paginationSettings) : IRequestHandler<GetOrganizationB2BUsersQuery, PagedResult<B2BUserDto>>
{

    public async Task<PagedResult<B2BUserDto>> Handle(GetOrganizationB2BUsersQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Retrieving B2B users for OrganizationId: {OrganizationId}, Status: {Status}, Page: {Page}, PageSize: {PageSize}",
            request.OrganizationId, request.Status, request.Page, request.PageSize);

        // ✅ BOLUM 3.4: Pagination limit kontrolü (ZORUNLU)
        var pageSize = request.PageSize > paginationSettings.Value.MaxPageSize ? paginationSettings.Value.MaxPageSize : request.PageSize;
        var page = request.Page < 1 ? 1 : request.Page;

        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        // ✅ PERFORMANCE: AsSplitQuery to avoid Cartesian Explosion (multiple Include'lar)
        // ✅ PERFORMANCE: Removed manual !b.IsDeleted check (Global Query Filter handles it)
        var query = context.Set<B2BUser>()
            .AsNoTracking()
            .AsSplitQuery() // ✅ BOLUM 8.1.4: Query Splitting - Multiple Include'lar için
            .Include(b => b.User)
            .Include(b => b.Organization)
            .Where(b => b.OrganizationId == request.OrganizationId);

        if (!string.IsNullOrEmpty(request.Status))
        {
            // ✅ BOLUM 1.2: Enum kullanımı (string Status YASAK)
            if (Enum.TryParse<EntityStatus>(request.Status, true, out var statusEnum))
            {
                query = query.Where(b => b.Status == statusEnum);
            }
        }

        // ✅ PERFORMANCE: TotalCount için ayrı query (CountAsync)
        var totalCount = await query.CountAsync(cancellationToken);

        var b2bUsers = await query
            .OrderBy(b => b.User.FirstName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullanımı (manuel mapping yerine)
        var items = mapper.Map<List<B2BUserDto>>(b2bUsers);

        // ✅ BOLUM 3.4: Pagination (ZORUNLU) - PagedResult döndürüyor
        return new PagedResult<B2BUserDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}

