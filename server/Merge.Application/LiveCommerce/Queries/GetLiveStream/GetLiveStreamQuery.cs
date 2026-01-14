using MediatR;
using Merge.Application.DTOs.LiveCommerce;

namespace Merge.Application.LiveCommerce.Queries.GetLiveStream;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetLiveStreamQuery(Guid Id) : IRequest<LiveStreamDto?>;

