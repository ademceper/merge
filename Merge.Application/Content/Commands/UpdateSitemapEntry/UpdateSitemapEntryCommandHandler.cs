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

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class UpdateSitemapEntryCommandHandler : IRequestHandler<UpdateSitemapEntryCommand, bool>
{
    private readonly Merge.Application.Interfaces.IRepository<SitemapEntry> _sitemapEntryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;
    private readonly ILogger<UpdateSitemapEntryCommandHandler> _logger;
    private const string CACHE_KEY_SITEMAP_ENTRIES = "sitemap_entries_all";
    private const string CACHE_KEY_SITEMAP_XML = "sitemap_xml";

    public UpdateSitemapEntryCommandHandler(
        Merge.Application.Interfaces.IRepository<SitemapEntry> sitemapEntryRepository,
        IUnitOfWork unitOfWork,
        ICacheService cache,
        ILogger<UpdateSitemapEntryCommandHandler> logger)
    {
        _sitemapEntryRepository = sitemapEntryRepository;
        _unitOfWork = unitOfWork;
        _cache = cache;
        _logger = logger;
    }

    public async Task<bool> Handle(UpdateSitemapEntryCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating sitemap entry. EntryId: {EntryId}", request.Id);

        // ✅ ARCHITECTURE: Transaction başlat - atomic operation
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var entry = await _sitemapEntryRepository.GetByIdAsync(request.Id, cancellationToken);
            if (entry == null)
            {
                _logger.LogWarning("Sitemap entry not found. EntryId: {EntryId}", request.Id);
                return false;
            }

            // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
            if (!string.IsNullOrEmpty(request.Url))
                entry.UpdateUrl(request.Url);
            if (!string.IsNullOrEmpty(request.ChangeFrequency) || request.Priority.HasValue)
                entry.UpdateSitemapSettings(
                    request.ChangeFrequency ?? entry.ChangeFrequency,
                    request.Priority ?? entry.Priority);

            await _sitemapEntryRepository.UpdateAsync(entry, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            // ✅ BOLUM 10.2: Cache invalidation
            await _cache.RemoveAsync(CACHE_KEY_SITEMAP_ENTRIES, cancellationToken);
            await _cache.RemoveAsync(CACHE_KEY_SITEMAP_XML, cancellationToken);

            _logger.LogInformation("Sitemap entry updated. EntryId: {EntryId}", request.Id);

            return true;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            _logger.LogError(ex, "Concurrency conflict while updating sitemap entry Id: {EntryId}", request.Id);
            throw new BusinessException("Sitemap entry güncelleme çakışması. Başka bir kullanıcı aynı entry'yi güncelledi. Lütfen tekrar deneyin.");
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            _logger.LogError(ex, "Error updating sitemap entry Id: {EntryId}", request.Id);
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}

