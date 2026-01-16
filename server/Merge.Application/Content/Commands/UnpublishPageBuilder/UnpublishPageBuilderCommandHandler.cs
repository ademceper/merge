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
using IRepository = Merge.Application.Interfaces.IRepository<Merge.Domain.Modules.Content.PageBuilder>;

namespace Merge.Application.Content.Commands.UnpublishPageBuilder;

public class UnpublishPageBuilderCommandHandler(
    IRepository pageBuilderRepository,
    IUnitOfWork unitOfWork,
    ICacheService cache,
    ILogger<UnpublishPageBuilderCommandHandler> logger) : IRequestHandler<UnpublishPageBuilderCommand, bool>
{
    private const string CACHE_KEY_ALL_PAGES = "page_builders_all";
    private const string CACHE_KEY_ACTIVE_PAGES = "page_builders_active";

    public async Task<bool> Handle(UnpublishPageBuilderCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Unpublishing page builder. PageBuilderId: {PageBuilderId}", request.Id);

        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var pageBuilder = await pageBuilderRepository.GetByIdAsync(request.Id, cancellationToken);
            if (pageBuilder == null)
            {
                logger.LogWarning("Page builder not found for unpublishing. PageBuilderId: {PageBuilderId}", request.Id);
                return false;
            }

            if (request.PerformedBy.HasValue && pageBuilder.AuthorId.HasValue && pageBuilder.AuthorId.Value != request.PerformedBy.Value)
            {
                logger.LogWarning("Unauthorized attempt to unpublish page builder {PageBuilderId} by user {UserId}. Page belongs to {AuthorId}",
                    request.Id, request.PerformedBy.Value, pageBuilder.AuthorId.Value);
                throw new BusinessException("Bu page builder'ı yayından kaldırma yetkiniz bulunmamaktadır.");
            }

            pageBuilder.Unpublish();

            await pageBuilderRepository.UpdateAsync(pageBuilder, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            await cache.RemoveAsync(CACHE_KEY_ALL_PAGES, cancellationToken);
            await cache.RemoveAsync(CACHE_KEY_ACTIVE_PAGES, cancellationToken);
            await cache.RemoveAsync($"page_builder_{pageBuilder.Id}", cancellationToken);
            await cache.RemoveAsync($"page_builder_slug_{pageBuilder.Slug}", cancellationToken);

            logger.LogInformation("Page builder unpublished. PageBuilderId: {PageBuilderId}", request.Id);
            return true;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            logger.LogError(ex, "Concurrency conflict while unpublishing page builder {PageBuilderId}", request.Id);
            throw new BusinessException("Page builder yayından kaldırma çakışması. Lütfen tekrar deneyin.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error unpublishing page builder {PageBuilderId}", request.Id);
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}

