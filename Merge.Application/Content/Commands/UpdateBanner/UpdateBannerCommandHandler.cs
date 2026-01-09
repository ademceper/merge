using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Marketing;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;

namespace Merge.Application.Content.Commands.UpdateBanner;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class UpdateBannerCommandHandler : IRequestHandler<UpdateBannerCommand, BannerDto>
{
    private readonly IRepository<Banner> _bannerRepository;
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;
    private readonly IMapper _mapper;
    private readonly ILogger<UpdateBannerCommandHandler> _logger;
    private const string CACHE_KEY_BANNER_BY_ID = "banner_";
    private const string CACHE_KEY_ACTIVE_BANNERS = "banners_active_";
    private const string CACHE_KEY_ALL_BANNERS = "banners_all_";

    public UpdateBannerCommandHandler(
        IRepository<Banner> bannerRepository,
        IDbContext context,
        IUnitOfWork unitOfWork,
        ICacheService cache,
        IMapper mapper,
        ILogger<UpdateBannerCommandHandler> logger)
    {
        _bannerRepository = bannerRepository;
        _context = context;
        _unitOfWork = unitOfWork;
        _cache = cache;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<BannerDto> Handle(UpdateBannerCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating banner. BannerId: {BannerId}, Title: {Title}",
            request.Id, request.Title);

        // ✅ ARCHITECTURE: Transaction başlat - atomic operation
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var banner = await _bannerRepository.GetByIdAsync(request.Id, cancellationToken);
            if (banner == null)
            {
                _logger.LogWarning("Banner not found. BannerId: {BannerId}", request.Id);
                throw new NotFoundException("Banner", request.Id);
            }

            // Store old position for cache invalidation
            var oldPosition = banner.Position;

            // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
            banner.UpdateTitle(request.Title);
            banner.UpdateDescription(request.Description);
            banner.UpdateImageUrl(request.ImageUrl);
            banner.UpdateLinkUrl(request.LinkUrl);
            banner.UpdatePosition(request.Position);
            banner.UpdateSortOrder(request.SortOrder);
            banner.UpdateDateRange(request.StartDate, request.EndDate);
            banner.UpdateCategory(request.CategoryId);
            banner.UpdateProduct(request.ProductId);
            
            if (request.IsActive)
                banner.Activate();
            else
                banner.Deactivate();

            await _bannerRepository.UpdateAsync(banner, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            _logger.LogInformation("Banner updated successfully. BannerId: {BannerId}, Title: {Title}",
                banner.Id, banner.Title);

            // ✅ BOLUM 10.2: Cache invalidation - Remove all banner-related cache
            await _cache.RemoveAsync($"{CACHE_KEY_BANNER_BY_ID}{request.Id}", cancellationToken);
            if (oldPosition != request.Position)
            {
                await _cache.RemoveAsync($"{CACHE_KEY_ACTIVE_BANNERS}{oldPosition}", cancellationToken);
                await _cache.RemoveAsync($"{CACHE_KEY_ACTIVE_BANNERS}{request.Position}", cancellationToken);
            }
            else
            {
                await _cache.RemoveAsync($"{CACHE_KEY_ACTIVE_BANNERS}{request.Position}", cancellationToken);
            }
            await _cache.RemoveAsync(CACHE_KEY_ALL_BANNERS, cancellationToken);

            return _mapper.Map<BannerDto>(banner);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            _logger.LogError(ex, "Concurrency conflict while updating banner. BannerId: {BannerId}",
                request.Id);
            throw new BusinessException("Banner güncelleme çakışması. Başka bir kullanıcı aynı banner'ı güncelledi. Lütfen tekrar deneyin.");
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            _logger.LogError(ex, "Error updating banner. BannerId: {BannerId}",
                request.Id);
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
