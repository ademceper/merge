using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Content;
using Merge.Domain.ValueObjects;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;
using IRepository = Merge.Application.Interfaces.IRepository<Merge.Domain.Modules.Content.LandingPage>;

namespace Merge.Application.Content.Commands.UpdateLandingPage;

public class UpdateLandingPageCommandHandler(
    IRepository landingPageRepository,
    IDbContext context,
    IUnitOfWork unitOfWork,
    ICacheService cache,
    ILogger<UpdateLandingPageCommandHandler> logger) : IRequestHandler<UpdateLandingPageCommand, bool>
{
    private const string CACHE_KEY_ALL_PAGES = "landing_pages_all";
    private const string CACHE_KEY_ACTIVE_PAGES = "landing_pages_active";

    public async Task<bool> Handle(UpdateLandingPageCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Updating landing page. LandingPageId: {LandingPageId}", request.Id);

        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var landingPage = await landingPageRepository.GetByIdAsync(request.Id, cancellationToken);
            if (landingPage is null)
            {
                logger.LogWarning("Landing page not found. LandingPageId: {LandingPageId}", request.Id);
                return false;
            }

            // Note: RowVersion kontrolü genellikle HTTP ETag veya If-Match header ile yapılır
            // Burada sadece entity'nin RowVersion'ını kontrol ediyoruz

            if (request.PerformedBy.HasValue && landingPage.AuthorId.HasValue && landingPage.AuthorId.Value != request.PerformedBy.Value)
            {
                logger.LogWarning("Unauthorized attempt to update landing page {LandingPageId} by user {UserId}. Page belongs to {AuthorId}",
                    request.Id, request.PerformedBy.Value, landingPage.AuthorId.Value);
                throw new BusinessException("Bu landing page'i güncelleme yetkiniz bulunmamaktadır.");
            }

            if (!string.IsNullOrEmpty(request.Name))
            {
                landingPage.UpdateName(request.Name);
            }
            if (!string.IsNullOrEmpty(request.Title))
            {
                landingPage.UpdateTitle(request.Title);
            }
            if (!string.IsNullOrEmpty(request.Content))
            {
                landingPage.UpdateContent(request.Content);
            }
            if (request.Template is not null)
            {
                landingPage.UpdateTemplate(request.Template);
            }
            if (!string.IsNullOrEmpty(request.Status))
            {
                if (Enum.TryParse<ContentStatus>(request.Status, true, out var newStatus))
                {
                    landingPage.UpdateStatus(newStatus);
                }
            }
            if (request.StartDate.HasValue || request.EndDate.HasValue)
            {
                landingPage.UpdateSchedule(request.StartDate, request.EndDate);
            }
            if (request.MetaTitle is not null || request.MetaDescription is not null || request.OgImageUrl is not null)
            {
                landingPage.UpdateMetaInformation(request.MetaTitle, request.MetaDescription, request.OgImageUrl);
            }
            if (request.EnableABTesting.HasValue || request.TrafficSplit.HasValue)
            {
                var trafficSplit = request.TrafficSplit ?? landingPage.TrafficSplit;
                landingPage.UpdateABTestingSettings(request.EnableABTesting ?? landingPage.EnableABTesting, trafficSplit);
            }

            await landingPageRepository.UpdateAsync(landingPage, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            await cache.RemoveAsync(CACHE_KEY_ALL_PAGES, cancellationToken);
            await cache.RemoveAsync(CACHE_KEY_ACTIVE_PAGES, cancellationToken);
            await cache.RemoveAsync($"landing_page_{landingPage.Id}", cancellationToken);
            await cache.RemoveAsync($"landing_page_slug_{landingPage.Slug}", cancellationToken);

            logger.LogInformation("Landing page updated. LandingPageId: {LandingPageId}", request.Id);
            return true;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            logger.LogError(ex, "Concurrency conflict while updating landing page {LandingPageId}", request.Id);
            throw new BusinessException("Landing page güncelleme çakışması. Lütfen tekrar deneyin.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating landing page {LandingPageId}", request.Id);
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}

