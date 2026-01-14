using MediatR;
using Merge.Application.Common;
using Merge.Application.DTOs.Marketing;

namespace Merge.Application.Marketing.Queries.GetAllEmailSubscribers;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 3.4: Pagination (ZORUNLU)
public record GetAllEmailSubscribersQuery(bool? IsSubscribed, int PageNumber, int PageSize) : IRequest<PagedResult<EmailSubscriberDto>>;
