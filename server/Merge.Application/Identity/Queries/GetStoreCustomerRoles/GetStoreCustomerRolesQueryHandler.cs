using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Identity;
using Merge.Application.Interfaces;
using Merge.Domain.Modules.Identity;
using IDbContext = Merge.Application.Interfaces.IDbContext;

namespace Merge.Application.Identity.Queries.GetStoreCustomerRoles;

public class GetStoreCustomerRolesQueryHandler(
    IDbContext context,
    IMapper mapper,
    ILogger<GetStoreCustomerRolesQueryHandler> logger) : IRequestHandler<GetStoreCustomerRolesQuery, List<StoreCustomerRoleDto>>
{
    public async Task<List<StoreCustomerRoleDto>> Handle(GetStoreCustomerRolesQuery request, CancellationToken ct)
    {
        logger.LogInformation("Getting store customer roles. StoreId: {StoreId}, UserId: {UserId}", request.StoreId, request.UserId);

        var query = context.Set<StoreCustomerRole>()
            .AsNoTracking()
            .Include(scr => scr.Store)
            .Include(scr => scr.User)
            .Include(scr => scr.Role)
            .Where(scr => !scr.IsDeleted)
            .AsQueryable();

        if (request.StoreId.HasValue)
        {
            query = query.Where(scr => scr.StoreId == request.StoreId.Value);
        }

        if (request.UserId.HasValue)
        {
            query = query.Where(scr => scr.UserId == request.UserId.Value);
        }

        var storeCustomerRoles = await query
            .OrderBy(scr => scr.Store.StoreName)
            .ThenBy(scr => scr.Role.Name)
            .Select(scr => new StoreCustomerRoleDto(
                scr.Id,
                scr.StoreId,
                scr.Store.StoreName,
                scr.UserId,
                scr.User.Email ?? string.Empty,
                scr.RoleId,
                scr.Role.Name ?? string.Empty,
                scr.AssignedAt,
                scr.AssignedByUserId))
            .ToListAsync(ct);

        return storeCustomerRoles;
    }
}
