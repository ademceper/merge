using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Merge.Application.Common;
using Merge.Application.DTOs.User;
using Merge.Application.Interfaces;
using Merge.Application.Configuration;
using AutoMapper;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Identity;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Analytics.Queries.GetUsers;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class GetUsersQueryHandler(
    IDbContext context,
    ILogger<GetUsersQueryHandler> logger,
    IOptions<AnalyticsSettings> settings,
    IOptions<PaginationSettings> paginationSettings,
    IMapper mapper) : IRequestHandler<GetUsersQuery, PagedResult<UserDto>>
{
    private readonly PaginationSettings paginationConfig = paginationSettings.Value;

    public async Task<PagedResult<UserDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Fetching users. Page: {Page}, PageSize: {PageSize}, Role: {Role}", 
            request.Page, request.PageSize, request.Role);

        // ✅ BOLUM 3.4: Pagination limit kontrolü (config'den)
        // ✅ BOLUM 2.3: Hardcoded Values YASAK - Configuration kullanılıyor
        var pageSize = request.PageSize <= 0 ? paginationConfig.DefaultPageSize : request.PageSize;
        if (pageSize > paginationConfig.MaxPageSize) pageSize = paginationConfig.MaxPageSize;
        var page = request.Page < 1 ? 1 : request.Page;

        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        // ✅ PERFORMANCE: Removed manual !u.IsDeleted check (Global Query Filter handles it)
        var query = context.Users.AsNoTracking();

        if (!string.IsNullOrEmpty(request.Role))
        {
            // ✅ Identity framework'ün Role ve UserRole entity'leri IDbContext üzerinden erişiliyor
            // ✅ PERFORMANCE: .Any() YASAK - .cursorrules - Role'ü önce bulup join yap
            var role = await context.Roles
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Name == request.Role, cancellationToken);
            
            if (role != null)
            {
                var userIdsWithRole = await context.UserRoles
                    .AsNoTracking()
                    .Where(ur => ur.RoleId == role.Id)
                    .Select(ur => ur.UserId)
                    .ToListAsync(cancellationToken);
                
                if (userIdsWithRole.Count > 0)
                {
                    query = query.Where(u => userIdsWithRole.Contains(u.Id));
                }
                else
                {
                    // No users with this role, return empty result
                    query = query.Where(u => false);
                }
            }
            else
            {
                // Role not found, return empty result
                query = query.Where(u => false);
            }
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var users = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullanımı (manuel mapping yerine)
        return new PagedResult<UserDto>
        {
            Items = mapper.Map<List<UserDto>>(users),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}

