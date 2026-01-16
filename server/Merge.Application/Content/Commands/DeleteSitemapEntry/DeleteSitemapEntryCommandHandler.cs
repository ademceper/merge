using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Content;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;
using IRepository = Merge.Application.Interfaces.IRepository<Merge.Domain.Modules.Content.SitemapEntry>;

namespace Merge.Application.Content.Commands.DeleteSitemapEntry;

public class DeleteSitemapEntryCommandHandler(
    IRepository sitemapEntryRepository,
    IUnitOfWork unitOfWork,
    ICacheService cache,
    ILogger<DeleteSitemapEntryCommandHandler> logger) : IRequestHandler<DeleteSitemapEntryCommand, bool>
{
    private const string CACHE_KEY_SITEMAP_ENTRIES = "sitemap_entries_all";
    private const string CACHE_KEY_SITEMAP_XML = "sitemap_xml";

    public async Task<bool> Handle(DeleteSitemapEntryCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Deleting sitemap entry. EntryId: {EntryId}", request.Id);

        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var entry = await sitemapEntryRepository.GetByIdAsync(request.Id, cancellationToken);
            if (entry == null)
            {
                logger.LogWarning("Sitemap entry not found for deletion. EntryId: {EntryId}", request.Id);
                return false;
            }

            entry.MarkAsDeleted();

            await sitemapEntryRepository.UpdateAsync(entry, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            await cache.RemoveAsync(CACHE_KEY_SITEMAP_ENTRIES, cancellationToken);
            await cache.RemoveAsync(CACHE_KEY_SITEMAP_XML, cancellationToken);

            logger.LogInformation("Sitemap entry deleted (soft delete). EntryId: {EntryId}", request.Id);

            return true;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            logger.LogError(ex, "Concurrency conflict while deleting sitemap entry Id: {EntryId}", request.Id);
            throw new BusinessException("Sitemap entry silme çakışması. Başka bir kullanıcı aynı entry'yi güncelledi. Lütfen tekrar deneyin.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting sitemap entry Id: {EntryId}", request.Id);
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}

