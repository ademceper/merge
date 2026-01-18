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
using Merge.Domain.Modules.Marketplace;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;
using IRepository = Merge.Application.Interfaces.IRepository<Merge.Domain.Modules.Content.Banner>;

namespace Merge.Application.Content.Commands.UpdateBanner;

public class UpdateBannerCommandHandler(
    IRepository bannerRepository,
    IDbContext context,
    IUnitOfWork unitOfWork,
    ICacheService cache,
    IMapper mapper,
    ILogger<UpdateBannerCommandHandler> logger) : IRequestHandler<UpdateBannerCommand, BannerDto>
{
    private const string CACHE_KEY_BANNER_BY_ID = "banner_";
    private const string CACHE_KEY_ACTIVE_BANNERS = "banners_active_";
    private const string CACHE_KEY_ALL_BANNERS = "banners_all_";

    public async Task<BannerDto> Handle(UpdateBannerCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Updating banner. BannerId: {BannerId}, Title: {Title}",
            request.Id, request.Title);

        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var banner = await bannerRepository.GetByIdAsync(request.Id, cancellationToken);
            if (banner is null)
            {
                logger.LogWarning("Banner not found. BannerId: {BannerId}", request.Id);
                throw new NotFoundException("Banner", request.Id);
            }

            // Store old position for cache invalidation
            var oldPosition = banner.Position;

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

            await bannerRepository.UpdateAsync(banner, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            logger.LogInformation("Banner updated successfully. BannerId: {BannerId}, Title: {Title}",
                banner.Id, banner.Title);

            await cache.RemoveAsync($"{CACHE_KEY_BANNER_BY_ID}{request.Id}", cancellationToken);
            if (oldPosition != request.Position)
            {
                await cache.RemoveAsync($"{CACHE_KEY_ACTIVE_BANNERS}{oldPosition}", cancellationToken);
                await cache.RemoveAsync($"{CACHE_KEY_ACTIVE_BANNERS}{request.Position}", cancellationToken);
            }
            else
            {
                await cache.RemoveAsync($"{CACHE_KEY_ACTIVE_BANNERS}{request.Position}", cancellationToken);
            }
            await cache.RemoveAsync(CACHE_KEY_ALL_BANNERS, cancellationToken);

            return mapper.Map<BannerDto>(banner);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            logger.LogError(ex, "Concurrency conflict while updating banner. BannerId: {BannerId}",
                request.Id);
            throw new BusinessException("Banner güncelleme çakışması. Başka bir kullanıcı aynı banner'ı güncelledi. Lütfen tekrar deneyin.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating banner. BannerId: {BannerId}",
                request.Id);
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
