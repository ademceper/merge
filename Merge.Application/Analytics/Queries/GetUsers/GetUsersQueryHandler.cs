using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Merge.Application.Common;
using Merge.Application.DTOs.User;
using Merge.Application.Interfaces;
using Merge.Application.Configuration;
using AutoMapper;

namespace Merge.Application.Analytics.Queries.GetUsers;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, PagedResult<UserDto>>
{
    private readonly IDbContext _context;
    private readonly ILogger<GetUsersQueryHandler> _logger;
    private readonly AnalyticsSettings _settings;
    private readonly PaginationSettings _paginationSettings;
    private readonly IMapper _mapper;

    public GetUsersQueryHandler(
        IDbContext context,
        ILogger<GetUsersQueryHandler> logger,
        IOptions<AnalyticsSettings> settings,
        IOptions<PaginationSettings> paginationSettings,
        IMapper mapper)
    {
        _context = context;
        _logger = logger;
        _settings = settings.Value;
        _paginationSettings = paginationSettings.Value;
        _mapper = mapper;
    }

    public async Task<PagedResult<UserDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching users. Page: {Page}, PageSize: {PageSize}, Role: {Role}", 
            request.Page, request.PageSize, request.Role);

        // ✅ BOLUM 3.4: Pagination limit kontrolü (config'den)
        // ✅ BOLUM 2.3: Hardcoded Values YASAK - Configuration kullanılıyor
        var pageSize = request.PageSize <= 0 ? _settings.DefaultPageSize : request.PageSize;
        if (pageSize > _paginationSettings.MaxPageSize) pageSize = _paginationSettings.MaxPageSize;
        var page = request.Page < 1 ? 1 : request.Page;

        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        // ✅ PERFORMANCE: Removed manual !u.IsDeleted check (Global Query Filter handles it)
        var query = _context.Users.AsNoTracking();

        if (!string.IsNullOrEmpty(request.Role))
        {
            // ✅ Identity framework'ün Role ve UserRole entity'leri IDbContext üzerinden erişiliyor
            // ✅ PERFORMANCE: .Any() YASAK - .cursorrules - Role'ü önce bulup join yap
            var role = await _context.Roles
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Name == request.Role, cancellationToken);
            
            if (role != null)
            {
                var userIdsWithRole = await _context.UserRoles
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
            Items = _mapper.Map<List<UserDto>>(users),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}

