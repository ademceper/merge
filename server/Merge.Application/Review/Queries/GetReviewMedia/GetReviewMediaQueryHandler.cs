using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Marketing;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Review.Queries.GetReviewMedia;

public class GetReviewMediaQueryHandler(IDbContext context, IMapper mapper, ILogger<GetReviewMediaQueryHandler> logger) : IRequestHandler<GetReviewMediaQuery, IEnumerable<ReviewMediaDto>>
{

    public async Task<IEnumerable<ReviewMediaDto>> Handle(GetReviewMediaQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Fetching review media. ReviewId: {ReviewId}", request.ReviewId);

        var media = await context.Set<ReviewMedia>()
            .AsNoTracking()
            .Where(m => m.ReviewId == request.ReviewId)
            .OrderBy(m => m.DisplayOrder)
            .ToListAsync(cancellationToken);

        return mapper.Map<IEnumerable<ReviewMediaDto>>(media);
    }
}
