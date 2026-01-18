using MediatR;
using Merge.Application.DTOs.Support;
using Merge.Application.Common;

namespace Merge.Application.Support.Queries.GetAllCustomerCommunications;

public record GetAllCustomerCommunicationsQuery(
    string? CommunicationType = null,
    string? Channel = null,
    Guid? UserId = null,
    DateTime? StartDate = null,
    DateTime? EndDate = null,
    int Page = 1,
    int PageSize = 20
) : IRequest<PagedResult<CustomerCommunicationDto>>;
