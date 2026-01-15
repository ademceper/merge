using MediatR;
using Merge.Application.DTOs.LiveCommerce;
using Merge.Application.Common;

namespace Merge.Application.LiveCommerce.Queries.GetActiveStreams;

public record GetActiveStreamsQuery(
    int Page = 1,
    int PageSize = 20) : IRequest<PagedResult<LiveStreamDto>>;
