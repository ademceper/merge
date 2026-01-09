using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Content;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;

namespace Merge.Application.Content.Commands.CreateSitemapEntry;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class CreateSitemapEntryCommandHandler : IRequestHandler<CreateSitemapEntryCommand, SitemapEntryDto>
{
    private readonly IRepository<SitemapEntry> _sitemapEntryRepository;
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateSitemapEntryCommandHandler> _logger;
    private const string CACHE_KEY_SITEMAP_ENTRIES = "sitemap_entries_all";
    private const string CACHE_KEY_SITEMAP_XML = "sitemap_xml";

    public CreateSitemapEntryCommandHandler(
        IRepository<SitemapEntry> sitemapEntryRepository,
        IDbContext context,
        IUnitOfWork unitOfWork,
        ICacheService cache,
        IMapper mapper,
        ILogger<CreateSitemapEntryCommandHandler> logger)
    {
        _sitemapEntryRepository = sitemapEntryRepository;
        _context = context;
        _unitOfWork = unitOfWork;
        _cache = cache;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<SitemapEntryDto> Handle(CreateSitemapEntryCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating sitemap entry. Url: {Url}, PageType: {PageType}",
            request.Url, request.PageType);

        // ✅ ARCHITECTURE: Transaction başlat - atomic operation
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
            var entry = SitemapEntry.Create(
                request.Url,
                request.PageType,
                request.EntityId,
                request.ChangeFrequency,
                request.Priority,
                isActive: true);

            entry = await _sitemapEntryRepository.AddAsync(entry, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            // ✅ PERFORMANCE: Reload with AsNoTracking
            var reloadedEntry = await _context.Set<SitemapEntry>()
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == entry.Id, cancellationToken);

            if (reloadedEntry == null)
            {
                _logger.LogWarning("Sitemap entry {EntryId} not found after creation", entry.Id);
                throw new NotFoundException("Sitemap Entry", entry.Id);
            }

            // ✅ BOLUM 10.2: Cache invalidation
            await _cache.RemoveAsync(CACHE_KEY_SITEMAP_ENTRIES, cancellationToken);
            await _cache.RemoveAsync(CACHE_KEY_SITEMAP_XML, cancellationToken);

            _logger.LogInformation("Sitemap entry created. EntryId: {EntryId}, Url: {Url}",
                entry.Id, request.Url);

            return _mapper.Map<SitemapEntryDto>(reloadedEntry);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            _logger.LogError(ex, "Concurrency conflict while creating sitemap entry. Url: {Url}", request.Url);
            throw new BusinessException("Sitemap entry oluşturma çakışması. Lütfen tekrar deneyin.");
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            _logger.LogError(ex, "Error creating sitemap entry. Url: {Url}", request.Url);
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}

