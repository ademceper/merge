using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Merge.Application.Interfaces;
using Merge.Application.Interfaces.Marketing;
using Merge.Application.DTOs.Marketing;
using Merge.Domain.Entities;
using Merge.Domain.Enums;

namespace Merge.Application.Services.Marketing;

public class ReviewMediaService : IReviewMediaService
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ReviewMediaService(IDbContext context, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ReviewMediaDto> AddMediaToReviewAsync(Guid reviewId, string url, string mediaType, string? thumbnailUrl = null)
    {
        var media = new ReviewMedia
        {
            ReviewId = reviewId,
            MediaType = Enum.Parse<ReviewMediaType>(mediaType, true),
            Url = url,
            ThumbnailUrl = thumbnailUrl ?? url
        };

        await _context.Set<ReviewMedia>().AddAsync(media);
        await _unitOfWork.SaveChangesAsync();

        // ✅ PERFORMANCE: Reload in one query (N+1 fix)
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !m.IsDeleted (Global Query Filter)
        var createdMedia = await _context.Set<ReviewMedia>()
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == media.Id);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<ReviewMediaDto>(createdMedia!);
    }

    public async Task<IEnumerable<ReviewMediaDto>> GetReviewMediaAsync(Guid reviewId)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !m.IsDeleted (Global Query Filter)
        var media = await _context.Set<ReviewMedia>()
            .AsNoTracking()
            .Where(m => m.ReviewId == reviewId)
            .OrderBy(m => m.DisplayOrder)
            .ToListAsync();

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<IEnumerable<ReviewMediaDto>>(media);
    }

    public async Task DeleteReviewMediaAsync(Guid mediaId)
    {
        // ✅ PERFORMANCE: FindAsync yerine FirstOrDefaultAsync (Global Query Filter)
        var media = await _context.Set<ReviewMedia>()
            .FirstOrDefaultAsync(m => m.Id == mediaId);
        if (media != null)
        {
            media.IsDeleted = true;
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
