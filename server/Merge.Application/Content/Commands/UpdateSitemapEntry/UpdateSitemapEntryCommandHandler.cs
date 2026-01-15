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

namespace Merge.Application.Content.Commands.UpdateSitemapEntry;

public class UpdateSitemapEntryCommandHandler(
    Merge.Application.Interfaces.IRepository<SitemapEntry> sitemapEntryRepository,
    IUnitOfWork unitOfWork,
    ICacheService cache,
    ILogger<UpdateSitemapEntryCommandHandler> logger) : IRequestHandler<UpdateSitemapEntryCommand, bool>
{
    private const string CACHE_KEY_SITEMAP_ENTRIES = "sitemap_entries_all";
    private const string CACHE_KEY_SITEMAP_XML = "sitemap_xml";

    public async Task<bool> Handle(UpdateSitemapEntryCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Updating sitemap entry. EntryId: {EntryId}", request.Id);

        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var entry = await sitemapEntryRepository.GetByIdAsync(request.Id, cancellationToken);
            if (entry == null)
            {
                logger.LogWarning("Sitemap entry not found. EntryId: {EntryId}", request.Id);
                return false;
            }

            if (!string.IsNullOrEmpty(request.Url))
                entry.UpdateUrl(request.Url);
            if (!string.IsNullOrEmpty(request.ChangeFrequency) || request.Priority.HasValue)
                entry.UpdateSitemapSettings(
                    request.ChangeFrequency ?? entry.ChangeFrequency,
                    request.Priority ?? entry.Priority);

            await sitemapEntryRepository.UpdateAsync(entry, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            await cache.RemoveAsync(CACHE_KEY_SITEMAP_ENTRIES, cancellationToken);
            await cache.RemoveAsync(CACHE_KEY_SITEMAP_XML, cancellationToken);

            logger.LogInformation("Sitemap entry updated. EntryId: {EntryId}", request.Id);

            return true;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            logger.LogError(ex, "Concurrency conflict while updating sitemap entry Id: {EntryId}", request.Id);
            throw new BusinessException("Sitemap entry güncelleme çakışması. Başka bir kullanıcı aynı entry'yi güncelledi. Lütfen tekrar deneyin.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating sitemap entry Id: {EntryId}", request.Id);
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}

