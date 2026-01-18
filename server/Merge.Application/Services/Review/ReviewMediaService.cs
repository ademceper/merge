using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Merge.Application.Interfaces;
using Merge.Application.Interfaces.Marketing;
using Merge.Application.DTOs.Marketing;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Services.Marketing;

public class ReviewMediaService(IDbContext context, IUnitOfWork unitOfWork, IMapper mapper) : IReviewMediaService
{

    public async Task<ReviewMediaDto> AddMediaToReviewAsync(Guid reviewId, string url, string mediaType, string? thumbnailUrl = null, CancellationToken cancellationToken = default)
    {
        var mediaTypeEnum = Enum.Parse<ReviewMediaType>(mediaType, true);
        var media = ReviewMedia.Create(
            reviewId,
            mediaTypeEnum,
            url,
            thumbnailUrl);

        await context.Set<ReviewMedia>().AddAsync(media, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var createdMedia = await context.Set<ReviewMedia>()
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == media.Id, cancellationToken);

        return mapper.Map<ReviewMediaDto>(createdMedia!);
    }

    public async Task<IEnumerable<ReviewMediaDto>> GetReviewMediaAsync(Guid reviewId, CancellationToken cancellationToken = default)
    {
        var media = await context.Set<ReviewMedia>()
            .AsNoTracking()
            .Where(m => m.ReviewId == reviewId)
            .OrderBy(m => m.DisplayOrder)
            .ToListAsync(cancellationToken);

        return mapper.Map<IEnumerable<ReviewMediaDto>>(media);
    }

    public async Task DeleteReviewMediaAsync(Guid mediaId, CancellationToken cancellationToken = default)
    {
        var media = await context.Set<ReviewMedia>()
            .FirstOrDefaultAsync(m => m.Id == mediaId, cancellationToken);
        if (media != null)
        {
            media.MarkAsDeleted();
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
