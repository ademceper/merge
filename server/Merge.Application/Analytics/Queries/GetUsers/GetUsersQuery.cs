using MediatR;
using Merge.Application.Common;
using Merge.Application.DTOs.User;
using Merge.Domain.Modules.Identity;

namespace Merge.Application.Analytics.Queries.GetUsers;

public record GetUsersQuery(
    int Page = 1,
    int PageSize = 0,
    string? Role = null
) : IRequest<PagedResult<UserDto>>;

