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

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<ReviewMediaDto> AddMediaToReviewAsync(Guid reviewId, string url, string mediaType, string? thumbnailUrl = null, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
        var mediaTypeEnum = Enum.Parse<ReviewMediaType>(mediaType, true);
        var media = ReviewMedia.Create(
            reviewId,
            mediaTypeEnum,
            url,
            thumbnailUrl);

        await context.Set<ReviewMedia>().AddAsync(media, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ PERFORMANCE: Reload in one query (N+1 fix)
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !m.IsDeleted (Global Query Filter)
        var createdMedia = await context.Set<ReviewMedia>()
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == media.Id, cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return mapper.Map<ReviewMediaDto>(createdMedia!);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<IEnumerable<ReviewMediaDto>> GetReviewMediaAsync(Guid reviewId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !m.IsDeleted (Global Query Filter)
        var media = await context.Set<ReviewMedia>()
            .AsNoTracking()
            .Where(m => m.ReviewId == reviewId)
            .OrderBy(m => m.DisplayOrder)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return mapper.Map<IEnumerable<ReviewMediaDto>>(media);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task DeleteReviewMediaAsync(Guid mediaId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: FindAsync yerine FirstOrDefaultAsync (Global Query Filter)
        var media = await context.Set<ReviewMedia>()
            .FirstOrDefaultAsync(m => m.Id == mediaId, cancellationToken);
        if (media != null)
        {
            // ✅ BOLUM 1.1: Rich Domain Model - Domain method kullan
            media.MarkAsDeleted();
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
