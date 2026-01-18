using MediatR;
using Merge.Application.Common;
using Merge.Application.DTOs.Marketing;

namespace Merge.Application.Marketing.Queries.GetAllEmailSubscribers;

public record GetAllEmailSubscribersQuery(bool? IsSubscribed, int PageNumber, int PageSize) : IRequest<PagedResult<EmailSubscriberDto>>;
