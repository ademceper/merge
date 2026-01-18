using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Marketing;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using ReviewEntity = Merge.Domain.Modules.Catalog.Review;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Review.Commands.AddReviewMedia;

public class AddReviewMediaCommandHandler(IDbContext context, IUnitOfWork unitOfWork, IMapper mapper, ILogger<AddReviewMediaCommandHandler> logger) : IRequestHandler<AddReviewMediaCommand, ReviewMediaDto>
{

    public async Task<ReviewMediaDto> Handle(AddReviewMediaCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Adding media to review. ReviewId: {ReviewId}, MediaType: {MediaType}",
            request.ReviewId, request.MediaType);

        // Review'ın var olduğunu kontrol et
        var review = await context.Set<ReviewEntity>()
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == request.ReviewId, cancellationToken);

        if (review is null)
        {
            throw new NotFoundException("Değerlendirme", request.ReviewId);
        }

        var mediaType = Enum.Parse<ReviewMediaType>(request.MediaType, true);
        var media = ReviewMedia.Create(
            request.ReviewId,
            mediaType,
            request.Url,
            request.ThumbnailUrl,
            request.FileSize,
            request.Width,
            request.Height,
            request.Duration,
            request.DisplayOrder);

        await context.Set<ReviewMedia>().AddAsync(media, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var createdMedia = await context.Set<ReviewMedia>()
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == media.Id, cancellationToken);

        logger.LogInformation(
            "Media added to review successfully. MediaId: {MediaId}, ReviewId: {ReviewId}",
            media.Id, request.ReviewId);

        return mapper.Map<ReviewMediaDto>(createdMedia!);
    }
}
