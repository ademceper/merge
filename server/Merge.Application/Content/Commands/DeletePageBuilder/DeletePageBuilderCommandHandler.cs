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

namespace Merge.Application.Content.Commands.DeletePageBuilder;

public class DeletePageBuilderCommandHandler(
    IRepository pageBuilderRepository,
    IUnitOfWork unitOfWork,
    ICacheService cache,
    ILogger<DeletePageBuilderCommandHandler> logger) : IRequestHandler<DeletePageBuilderCommand, bool>
{
    private const string CACHE_KEY_ALL_PAGES = "page_builders_all";
    private const string CACHE_KEY_ACTIVE_PAGES = "page_builders_active";

    public async Task<bool> Handle(DeletePageBuilderCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Deleting page builder. PageBuilderId: {PageBuilderId}", request.Id);

        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var pageBuilder = await pageBuilderRepository.GetByIdAsync(request.Id, cancellationToken);
            if (pageBuilder == null)
            {
                logger.LogWarning("Page builder not found for deletion. PageBuilderId: {PageBuilderId}", request.Id);
                return false;
            }

            if (request.PerformedBy.HasValue && pageBuilder.AuthorId.HasValue && pageBuilder.AuthorId.Value != request.PerformedBy.Value)
            {
                logger.LogWarning("Unauthorized attempt to delete page builder {PageBuilderId} by user {UserId}. Page belongs to {AuthorId}",
                    request.Id, request.PerformedBy.Value, pageBuilder.AuthorId.Value);
                throw new BusinessException("Bu page builder'ı silme yetkiniz bulunmamaktadır.");
            }

            pageBuilder.MarkAsDeleted();

            await pageBuilderRepository.UpdateAsync(pageBuilder, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            await cache.RemoveAsync(CACHE_KEY_ALL_PAGES, cancellationToken);
            await cache.RemoveAsync(CACHE_KEY_ACTIVE_PAGES, cancellationToken);
            await cache.RemoveAsync($"page_builder_{request.Id}", cancellationToken);
            await cache.RemoveAsync($"page_builder_slug_{pageBuilder.Slug}", cancellationToken);

            logger.LogInformation("Page builder deleted (soft delete). PageBuilderId: {PageBuilderId}", request.Id);
            return true;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            logger.LogError(ex, "Concurrency conflict while deleting page builder Id: {PageBuilderId}", request.Id);
            throw new BusinessException("Page builder silme çakışması. Başka bir kullanıcı aynı page builder'ı güncelledi. Lütfen tekrar deneyin.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting page builder Id: {PageBuilderId}", request.Id);
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}

