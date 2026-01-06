using MediatR;
using Merge.Application.Common;
using Merge.Application.DTOs.User;

namespace Merge.Application.Analytics.Queries.GetUsers;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetUsersQuery(
    int Page = 1,
    int PageSize = 0,
    string? Role = null
) : IRequest<PagedResult<UserDto>>;

