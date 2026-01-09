using MediatR;
using Merge.Application.DTOs.Marketing;

namespace Merge.Application.Content.Queries.GetBannerById;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetBannerByIdQuery(
    Guid Id
) : IRequest<BannerDto?>;

