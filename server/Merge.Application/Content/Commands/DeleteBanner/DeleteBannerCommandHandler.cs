using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Content;
using Merge.Domain.Modules.Marketplace;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;
using IRepository = Merge.Application.Interfaces.IRepository<Merge.Domain.Modules.Content.Banner>;

namespace Merge.Application.Content.Commands.DeleteBanner;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class DeleteBannerCommandHandler(
    IRepository bannerRepository,
    IDbContext context,
    IUnitOfWork unitOfWork,
    ICacheService cache,
    ILogger<DeleteBannerCommandHandler> logger) : IRequestHandler<DeleteBannerCommand, bool>
{
    private const string CACHE_KEY_BANNER_BY_ID = "banner_";
    private const string CACHE_KEY_ACTIVE_BANNERS = "banners_active_";
    private const string CACHE_KEY_ALL_BANNERS = "banners_all_";

    public async Task<bool> Handle(DeleteBannerCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Deleting banner. BannerId: {BannerId}", request.Id);

        // ✅ ARCHITECTURE: Transaction başlat - atomic operation
        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var banner = await bannerRepository.GetByIdAsync(request.Id, cancellationToken);
            if (banner == null)
            {
                logger.LogWarning("Banner not found. BannerId: {BannerId}", request.Id);
                return false;
            }

            // Store position for cache invalidation
            var position = banner.Position;

            // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı (soft delete)
            banner.MarkAsDeleted();

            await bannerRepository.UpdateAsync(banner, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            logger.LogInformation("Banner deleted successfully. BannerId: {BannerId}", request.Id);

            // ✅ BOLUM 10.2: Cache invalidation - Remove all banner-related cache
            await cache.RemoveAsync($"{CACHE_KEY_BANNER_BY_ID}{request.Id}", cancellationToken);
            await cache.RemoveAsync($"{CACHE_KEY_ACTIVE_BANNERS}{position}", cancellationToken);
            await cache.RemoveAsync(CACHE_KEY_ALL_BANNERS, cancellationToken);

            return true;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            logger.LogError(ex, "Concurrency conflict while deleting banner. BannerId: {BannerId}",
                request.Id);
            throw new BusinessException("Banner silme çakışması. Başka bir kullanıcı aynı banner'ı güncelledi. Lütfen tekrar deneyin.");
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            logger.LogError(ex, "Error deleting banner. BannerId: {BannerId}", request.Id);
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}

