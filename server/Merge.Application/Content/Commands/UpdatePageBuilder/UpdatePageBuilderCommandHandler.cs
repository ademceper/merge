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

namespace Merge.Application.Content.Commands.UpdatePageBuilder;

public class UpdatePageBuilderCommandHandler(
    IRepository pageBuilderRepository,
    IDbContext context,
    IUnitOfWork unitOfWork,
    ICacheService cache,
    ILogger<UpdatePageBuilderCommandHandler> logger) : IRequestHandler<UpdatePageBuilderCommand, bool>
{
    private const string CACHE_KEY_ALL_PAGES = "page_builders_all";
    private const string CACHE_KEY_ACTIVE_PAGES = "page_builders_active";

    public async Task<bool> Handle(UpdatePageBuilderCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Updating page builder. PageBuilderId: {PageBuilderId}", request.Id);

        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var pageBuilder = await pageBuilderRepository.GetByIdAsync(request.Id, cancellationToken);
            if (pageBuilder is null)
            {
                logger.LogWarning("Page builder not found. PageBuilderId: {PageBuilderId}", request.Id);
                return false;
            }

            if (request.PerformedBy.HasValue && pageBuilder.AuthorId.HasValue && pageBuilder.AuthorId.Value != request.PerformedBy.Value)
            {
                logger.LogWarning("Unauthorized attempt to update page builder {PageBuilderId} by user {UserId}. Page belongs to {AuthorId}",
                    request.Id, request.PerformedBy.Value, pageBuilder.AuthorId.Value);
                throw new BusinessException("Bu page builder'ı güncelleme yetkiniz bulunmamaktadır.");
            }

            if (!string.IsNullOrEmpty(request.Name)) pageBuilder.UpdateName(request.Name);
            if (!string.IsNullOrEmpty(request.Slug)) pageBuilder.UpdateSlug(request.Slug);
            if (!string.IsNullOrEmpty(request.Title)) pageBuilder.UpdateTitle(request.Title);
            if (!string.IsNullOrEmpty(request.Content)) pageBuilder.UpdateContent(request.Content);
            if (request.Template is not null) pageBuilder.UpdateTemplate(request.Template);
            if (request.PageType is not null) pageBuilder.UpdatePageType(request.PageType);
            if (request.RelatedEntityId.HasValue) pageBuilder.UpdateRelatedEntity(request.RelatedEntityId);
            if (request.MetaTitle is not null || request.MetaDescription is not null || request.OgImageUrl is not null)
                pageBuilder.UpdateMetaInformation(request.MetaTitle, request.MetaDescription, request.OgImageUrl);

            await pageBuilderRepository.UpdateAsync(pageBuilder, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            await cache.RemoveAsync(CACHE_KEY_ALL_PAGES, cancellationToken);
            await cache.RemoveAsync(CACHE_KEY_ACTIVE_PAGES, cancellationToken);
            await cache.RemoveAsync($"page_builder_{pageBuilder.Id}", cancellationToken);
            await cache.RemoveAsync($"page_builder_slug_{pageBuilder.Slug}", cancellationToken);

            logger.LogInformation("Page builder updated. PageBuilderId: {PageBuilderId}", request.Id);
            return true;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            logger.LogError(ex, "Concurrency conflict while updating page builder {PageBuilderId}", request.Id);
            throw new BusinessException("Page builder güncelleme çakışması. Lütfen tekrar deneyin.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating page builder {PageBuilderId}", request.Id);
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}

