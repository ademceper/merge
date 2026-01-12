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

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class AddReviewMediaCommandHandler : IRequestHandler<AddReviewMediaCommand, ReviewMediaDto>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<AddReviewMediaCommandHandler> _logger;

    public AddReviewMediaCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<AddReviewMediaCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ReviewMediaDto> Handle(AddReviewMediaCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Adding media to review. ReviewId: {ReviewId}, MediaType: {MediaType}",
            request.ReviewId, request.MediaType);

        // Review'ın var olduğunu kontrol et
        var review = await _context.Set<ReviewEntity>()
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == request.ReviewId, cancellationToken);

        if (review == null)
        {
            throw new NotFoundException("Değerlendirme", request.ReviewId);
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
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

        await _context.Set<ReviewMedia>().AddAsync(media, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ PERFORMANCE: Reload in one query (N+1 fix)
        var createdMedia = await _context.Set<ReviewMedia>()
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == media.Id, cancellationToken);

        _logger.LogInformation(
            "Media added to review successfully. MediaId: {MediaId}, ReviewId: {ReviewId}",
            media.Id, request.ReviewId);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<ReviewMediaDto>(createdMedia!);
    }
}
