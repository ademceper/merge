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

        var pageSize = request.PageSize <= 0 ? paginationConfig.DefaultPageSize : request.PageSize;
        if (pageSize > paginationConfig.MaxPageSize) pageSize = paginationConfig.MaxPageSize;
        var page = request.Page < 1 ? 1 : request.Page;

        var query = context.Users.AsNoTracking();

        if (!string.IsNullOrEmpty(request.Role))
        {
            var role = await context.Roles
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Name == request.Role, cancellationToken);
            
            if (role is not null)
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

        return new PagedResult<UserDto>
        {
            Items = mapper.Map<List<UserDto>>(users),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}

