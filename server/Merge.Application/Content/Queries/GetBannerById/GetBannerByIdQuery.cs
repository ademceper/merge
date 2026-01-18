using MediatR;
using Merge.Application.DTOs.Marketing;

namespace Merge.Application.Content.Queries.GetBannerById;

public record GetBannerByIdQuery(
    Guid Id
) : IRequest<BannerDto?>;

