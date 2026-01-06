using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Application.Common;
using Merge.Application.DTOs.User;
using Merge.Application.Interfaces.Analytics;

namespace Merge.Application.Analytics.Queries.GetUsers;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, PagedResult<UserDto>>
{
    private readonly IAdminService _adminService;
    private readonly ILogger<GetUsersQueryHandler> _logger;

    public GetUsersQueryHandler(
        IAdminService adminService,
        ILogger<GetUsersQueryHandler> logger)
    {
        _adminService = adminService;
        _logger = logger;
    }

    public async Task<PagedResult<UserDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching users. Page: {Page}, PageSize: {PageSize}, Role: {Role}", 
            request.Page, request.PageSize, request.Role);

        return await _adminService.GetUsersAsync(request.Page, request.PageSize, request.Role, cancellationToken);
    }
}

