using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Content;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Content;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;
using IRepository = Merge.Application.Interfaces.IRepository<Merge.Domain.Modules.Content.SitemapEntry>;

namespace Merge.Application.Content.Commands.CreateSitemapEntry;

public class CreateSitemapEntryCommandHandler(
    IRepository sitemapEntryRepository,
    IDbContext context,
    IUnitOfWork unitOfWork,
    ICacheService cache,
    IMapper mapper,
    ILogger<CreateSitemapEntryCommandHandler> logger) : IRequestHandler<CreateSitemapEntryCommand, SitemapEntryDto>
{
    private const string CACHE_KEY_SITEMAP_ENTRIES = "sitemap_entries_all";
    private const string CACHE_KEY_SITEMAP_XML = "sitemap_xml";

    public async Task<SitemapEntryDto> Handle(CreateSitemapEntryCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Creating sitemap entry. Url: {Url}, PageType: {PageType}",
            request.Url, request.PageType);

        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var entry = SitemapEntry.Create(
                request.Url,
                request.PageType,
                request.EntityId,
                request.ChangeFrequency,
                request.Priority,
                isActive: true);

            entry = await sitemapEntryRepository.AddAsync(entry, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            var reloadedEntry = await context.Set<SitemapEntry>()
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == entry.Id, cancellationToken);

            if (reloadedEntry == null)
            {
                logger.LogWarning("Sitemap entry {EntryId} not found after creation", entry.Id);
                throw new NotFoundException("Sitemap Entry", entry.Id);
            }

            await cache.RemoveAsync(CACHE_KEY_SITEMAP_ENTRIES, cancellationToken);
            await cache.RemoveAsync(CACHE_KEY_SITEMAP_XML, cancellationToken);

            logger.LogInformation("Sitemap entry created. EntryId: {EntryId}, Url: {Url}",
                entry.Id, request.Url);

            return mapper.Map<SitemapEntryDto>(reloadedEntry);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            logger.LogError(ex, "Concurrency conflict while creating sitemap entry. Url: {Url}", request.Url);
            throw new BusinessException("Sitemap entry oluşturma çakışması. Lütfen tekrar deneyin.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating sitemap entry. Url: {Url}", request.Url);
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}

