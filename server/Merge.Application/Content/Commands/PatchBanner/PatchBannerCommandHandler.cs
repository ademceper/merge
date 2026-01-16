using MediatR;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Content;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Content;
using IRepository = Merge.Application.Interfaces.IRepository;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Content.Commands.PatchBanner;

/// <summary>
/// Handler for PatchBannerCommand
/// HIGH-API-001: PATCH Support - Partial updates implementation
/// </summary>
public class PatchBannerCommandHandler(
    IRepository<Banner> bannerRepository,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ILogger<PatchBannerCommandHandler> logger) : IRequestHandler<PatchBannerCommand, BannerDto>
{
    private const string CACHE_KEY_BANNER_BY_ID = "banner_";
    private const string CACHE_KEY_ACTIVE_BANNERS = "banners_active_";
    private const string CACHE_KEY_ALL_BANNERS = "banners_all_";

    public async Task<BannerDto> Handle(PatchBannerCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Patching banner. BannerId: {BannerId}", request.Id);

        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var banner = await bannerRepository.GetByIdAsync(request.Id, cancellationToken);
            if (banner == null)
            {
                logger.LogWarning("Banner not found. BannerId: {BannerId}", request.Id);
                throw new NotFoundException("Banner", request.Id);
            }

            var oldPosition = banner.Position;

            // Apply partial updates
            if (request.PatchDto.Title != null)
                banner.UpdateTitle(request.PatchDto.Title);
            if (request.PatchDto.Description != null)
                banner.UpdateDescription(request.PatchDto.Description);
            if (request.PatchDto.ImageUrl != null)
                banner.UpdateImageUrl(request.PatchDto.ImageUrl);
            if (request.PatchDto.LinkUrl != null)
                banner.UpdateLinkUrl(request.PatchDto.LinkUrl);
            if (request.PatchDto.Position != null)
                banner.UpdatePosition(request.PatchDto.Position);
            if (request.PatchDto.SortOrder.HasValue)
                banner.UpdateSortOrder(request.PatchDto.SortOrder.Value);
            if (request.PatchDto.StartDate.HasValue || request.PatchDto.EndDate.HasValue)
            {
                var startDate = request.PatchDto.StartDate ?? banner.StartDate;
                var endDate = request.PatchDto.EndDate ?? banner.EndDate;
                banner.UpdateDateRange(startDate, endDate);
            }
            if (request.PatchDto.CategoryId.HasValue)
                banner.UpdateCategory(request.PatchDto.CategoryId);
            if (request.PatchDto.ProductId.HasValue)
                banner.UpdateProduct(request.PatchDto.ProductId);

            if (request.PatchDto.IsActive.HasValue)
            {
                if (request.PatchDto.IsActive.Value)
                    banner.Activate();
                else
                    banner.Deactivate();
            }

            await bannerRepository.UpdateAsync(banner, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            logger.LogInformation("Banner patched successfully. BannerId: {BannerId}", request.Id);

            return mapper.Map<BannerDto>(banner);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error patching banner. BannerId: {BannerId}", request.Id);
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
