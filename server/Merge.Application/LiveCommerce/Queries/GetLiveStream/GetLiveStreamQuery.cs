using MediatR;
using Merge.Application.DTOs.LiveCommerce;

namespace Merge.Application.LiveCommerce.Queries.GetLiveStream;

public record GetLiveStreamQuery(Guid Id) : IRequest<LiveStreamDto?>;
