using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Marketing;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Content;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;
using IRepository = Merge.Application.Interfaces.IRepository<Merge.Domain.Modules.Content.Banner>;

namespace Merge.Application.Content.Commands.CreateBanner;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class CreateBannerCommandHandler(
    IRepository bannerRepository,
    IDbContext context,
    IUnitOfWork unitOfWork,
    ICacheService cache,
    IMapper mapper,
    ILogger<CreateBannerCommandHandler> logger) : IRequestHandler<CreateBannerCommand, BannerDto>
{
    private const string CACHE_KEY_BANNER_BY_ID = "banner_";
    private const string CACHE_KEY_ACTIVE_BANNERS = "banners_active_";
    private const string CACHE_KEY_ALL_BANNERS = "banners_all_";

    public async Task<BannerDto> Handle(CreateBannerCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Creating banner. Title: {Title}, Position: {Position}",
            request.Title, request.Position);

        // ✅ ARCHITECTURE: Transaction başlat - atomic operation
        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
            var banner = Banner.Create(
                request.Title,
                request.ImageUrl,
                request.Position,
                request.Description,
                request.LinkUrl,
                request.SortOrder,
                request.IsActive,
                request.StartDate,
                request.EndDate,
                request.CategoryId,
                request.ProductId);

            banner = await bannerRepository.AddAsync(banner, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            logger.LogInformation("Banner created successfully. BannerId: {BannerId}, Title: {Title}",
                banner.Id, banner.Title);

            // ✅ BOLUM 10.2: Cache invalidation - Remove all banner-related cache
            await cache.RemoveAsync($"{CACHE_KEY_BANNER_BY_ID}{banner.Id}", cancellationToken);
            await cache.RemoveAsync($"{CACHE_KEY_ACTIVE_BANNERS}{request.Position}", cancellationToken);
            await cache.RemoveAsync(CACHE_KEY_ALL_BANNERS, cancellationToken);

            return mapper.Map<BannerDto>(banner);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            logger.LogError(ex, "Concurrency conflict while creating banner. Title: {Title}, Position: {Position}",
                request.Title, request.Position);
            throw new BusinessException("Banner oluşturma çakışması. Lütfen tekrar deneyin.");
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            logger.LogError(ex, "Error creating banner. Title: {Title}, Position: {Position}",
                request.Title, request.Position);
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
