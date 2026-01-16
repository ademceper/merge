using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Content;
using Merge.Domain.ValueObjects;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;
using IRepository = Merge.Application.Interfaces.IRepository<Merge.Domain.Modules.Content.LandingPage>;

namespace Merge.Application.Content.Commands.PublishLandingPage;

public class PublishLandingPageCommandHandler(
    IRepository landingPageRepository,
    IUnitOfWork unitOfWork,
    ICacheService cache,
    ILogger<PublishLandingPageCommandHandler> logger) : IRequestHandler<PublishLandingPageCommand, bool>
{
    private const string CACHE_KEY_ALL_PAGES = "landing_pages_all";
    private const string CACHE_KEY_ACTIVE_PAGES = "landing_pages_active";

    public async Task<bool> Handle(PublishLandingPageCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Publishing landing page. LandingPageId: {LandingPageId}", request.Id);

        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var landingPage = await landingPageRepository.GetByIdAsync(request.Id, cancellationToken);
            if (landingPage == null)
            {
                logger.LogWarning("Landing page not found for publishing. LandingPageId: {LandingPageId}", request.Id);
                return false;
            }

            if (request.PerformedBy.HasValue && landingPage.AuthorId.HasValue && landingPage.AuthorId.Value != request.PerformedBy.Value)
            {
                logger.LogWarning("Unauthorized attempt to publish landing page {LandingPageId} by user {UserId}. Page belongs to {AuthorId}",
                    request.Id, request.PerformedBy.Value, landingPage.AuthorId.Value);
                throw new BusinessException("Bu landing page'i yayınlama yetkiniz bulunmamaktadır.");
            }

            landingPage.Publish();

            await landingPageRepository.UpdateAsync(landingPage, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            await cache.RemoveAsync(CACHE_KEY_ALL_PAGES, cancellationToken);
            await cache.RemoveAsync(CACHE_KEY_ACTIVE_PAGES, cancellationToken);
            await cache.RemoveAsync($"landing_page_{landingPage.Id}", cancellationToken);
            await cache.RemoveAsync($"landing_page_slug_{landingPage.Slug}", cancellationToken);

            logger.LogInformation("Landing page published. LandingPageId: {LandingPageId}", request.Id);
            return true;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            logger.LogError(ex, "Concurrency conflict while publishing landing page {LandingPageId}", request.Id);
            throw new BusinessException("Landing page yayınlama çakışması. Lütfen tekrar deneyin.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error publishing landing page {LandingPageId}", request.Id);
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}

