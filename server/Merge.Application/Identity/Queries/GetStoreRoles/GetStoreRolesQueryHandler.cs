using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Identity;
using Merge.Application.Interfaces;
using Merge.Domain.Modules.Identity;
using IDbContext = Merge.Application.Interfaces.IDbContext;

namespace Merge.Application.Identity.Queries.GetStoreRoles;

public class GetStoreRolesQueryHandler(
    IDbContext context,
    IMapper mapper,
    ILogger<GetStoreRolesQueryHandler> logger) : IRequestHandler<GetStoreRolesQuery, List<StoreRoleDto>>
{
    public async Task<List<StoreRoleDto>> Handle(GetStoreRolesQuery request, CancellationToken ct)
    {
        logger.LogInformation("Getting store roles. StoreId: {StoreId}, UserId: {UserId}", request.StoreId, request.UserId);

        var query = context.Set<StoreRole>()
            .AsNoTracking()
            .Include(sr => sr.Store)
            .Include(sr => sr.User)
            .Include(sr => sr.Role)
            .Where(sr => !sr.IsDeleted)
            .AsQueryable();

        if (request.StoreId.HasValue)
        {
            query = query.Where(sr => sr.StoreId == request.StoreId.Value);
        }

        if (request.UserId.HasValue)
        {
            query = query.Where(sr => sr.UserId == request.UserId.Value);
        }

        var storeRoles = await query
            .OrderBy(sr => sr.Store.StoreName)
            .ThenBy(sr => sr.Role.Name)
            .Select(sr => new StoreRoleDto(
                sr.Id,
                sr.StoreId,
                sr.Store.StoreName,
                sr.UserId,
                sr.User.Email ?? string.Empty,
                sr.RoleId,
                sr.Role.Name ?? string.Empty,
                sr.AssignedAt,
                sr.AssignedByUserId))
            .ToListAsync(ct);

        return storeRoles;
    }
}
