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

namespace Merge.Application.Content.Commands.DeleteBanner;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class DeleteBannerCommandHandler : IRequestHandler<DeleteBannerCommand, bool>
{
    private readonly Merge.Application.Interfaces.IRepository<Banner> _bannerRepository;
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;
    private readonly ILogger<DeleteBannerCommandHandler> _logger;
    private const string CACHE_KEY_BANNER_BY_ID = "banner_";
    private const string CACHE_KEY_ACTIVE_BANNERS = "banners_active_";
    private const string CACHE_KEY_ALL_BANNERS = "banners_all_";

    public DeleteBannerCommandHandler(
        Merge.Application.Interfaces.IRepository<Banner> bannerRepository,
        IDbContext context,
        IUnitOfWork unitOfWork,
        ICacheService cache,
        ILogger<DeleteBannerCommandHandler> logger)
    {
        _bannerRepository = bannerRepository;
        _context = context;
        _unitOfWork = unitOfWork;
        _cache = cache;
        _logger = logger;
    }

    public async Task<bool> Handle(DeleteBannerCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting banner. BannerId: {BannerId}", request.Id);

        // ✅ ARCHITECTURE: Transaction başlat - atomic operation
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var banner = await _bannerRepository.GetByIdAsync(request.Id, cancellationToken);
            if (banner == null)
            {
                _logger.LogWarning("Banner not found. BannerId: {BannerId}", request.Id);
                return false;
            }

            // Store position for cache invalidation
            var position = banner.Position;

            // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı (soft delete)
            banner.MarkAsDeleted();

            await _bannerRepository.UpdateAsync(banner, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            _logger.LogInformation("Banner deleted successfully. BannerId: {BannerId}", request.Id);

            // ✅ BOLUM 10.2: Cache invalidation - Remove all banner-related cache
            await _cache.RemoveAsync($"{CACHE_KEY_BANNER_BY_ID}{request.Id}", cancellationToken);
            await _cache.RemoveAsync($"{CACHE_KEY_ACTIVE_BANNERS}{position}", cancellationToken);
            await _cache.RemoveAsync(CACHE_KEY_ALL_BANNERS, cancellationToken);

            return true;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            _logger.LogError(ex, "Concurrency conflict while deleting banner. BannerId: {BannerId}",
                request.Id);
            throw new BusinessException("Banner silme çakışması. Başka bir kullanıcı aynı banner'ı güncelledi. Lütfen tekrar deneyin.");
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            _logger.LogError(ex, "Error deleting banner. BannerId: {BannerId}", request.Id);
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}

