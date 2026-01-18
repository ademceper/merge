using MediatR;
using Merge.Application.Common;
using Merge.Application.DTOs.B2B;

namespace Merge.Application.B2B.Queries.GetOrganizationB2BUsers;

public record GetOrganizationB2BUsersQuery(
    Guid OrganizationId,
    string? Status = null,
    int Page = 1,
    int PageSize = 20
) : IRequest<PagedResult<B2BUserDto>>;

