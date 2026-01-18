using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Review;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.ValueObjects;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using ReviewEntity = Merge.Domain.Modules.Catalog.Review;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Ordering;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Review.Commands.CreateReview;

public class CreateReviewCommandHandler(IDbContext context, IUnitOfWork unitOfWork, IMapper mapper, ILogger<CreateReviewCommandHandler> logger) : IRequestHandler<CreateReviewCommand, ReviewDto>
{

    public async Task<ReviewDto> Handle(CreateReviewCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Creating review. UserId: {UserId}, ProductId: {ProductId}, Rating: {Rating}",
            request.UserId, request.ProductId, request.Rating);


        var hasOrder = await context.Set<OrderItem>()
            .AnyAsync(oi => oi.ProductId == request.ProductId &&
                          oi.Order.UserId == request.UserId &&
                          oi.Order.PaymentStatus == PaymentStatus.Completed, cancellationToken);

        var rating = new Rating(request.Rating);
        var review = ReviewEntity.Create(
            request.UserId,
            request.ProductId,
            rating,
            request.Title,
            request.Comment,
            hasOrder
        );

        await context.Set<ReviewEntity>().AddAsync(review, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Not: Review henüz approved değil, bu yüzden product rating güncellemesi yapılmıyor
        // Product rating, review approve edildiğinde (ApproveReviewCommandHandler'da) güncellenecek

        review = await context.Set<ReviewEntity>()
            .AsNoTracking()
            .Include(r => r.User)
            .Include(r => r.Product)
            .FirstOrDefaultAsync(r => r.Id == review.Id, cancellationToken);

        logger.LogInformation(
            "Review created successfully. ReviewId: {ReviewId}, ProductId: {ProductId}, UserId: {UserId}, Rating: {Rating}",
            review!.Id, request.ProductId, request.UserId, request.Rating);

        return mapper.Map<ReviewDto>(review);
    }
}
