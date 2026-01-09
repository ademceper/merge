using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Content;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Enums;

namespace Merge.Application.Content.Commands.CreatePageBuilder;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class CreatePageBuilderCommandHandler : IRequestHandler<CreatePageBuilderCommand, PageBuilderDto>
{
    private readonly IRepository<PageBuilder> _pageBuilderRepository;
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;
    private readonly IMapper _mapper;
    private readonly ILogger<CreatePageBuilderCommandHandler> _logger;
    private const string CACHE_KEY_ALL_PAGES = "page_builders_all";
    private const string CACHE_KEY_ACTIVE_PAGES = "page_builders_active";

    public CreatePageBuilderCommandHandler(
        IRepository<PageBuilder> pageBuilderRepository,
        IDbContext context,
        IUnitOfWork unitOfWork,
        ICacheService cache,
        IMapper mapper,
        ILogger<CreatePageBuilderCommandHandler> logger)
    {
        _pageBuilderRepository = pageBuilderRepository;
        _context = context;
        _unitOfWork = unitOfWork;
        _cache = cache;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<PageBuilderDto> Handle(CreatePageBuilderCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating page builder. AuthorId: {AuthorId}, Name: {Name}, Title: {Title}",
            request.AuthorId, request.Name, request.Title);

        // ✅ ARCHITECTURE: Transaction başlat - atomic operation
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            // Slug uniqueness check
            var slug = request.Slug ?? GenerateSlug(request.Name);
            if (await _context.Set<PageBuilder>().AnyAsync(pb => pb.Slug == slug, cancellationToken))
            {
                slug = $"{slug}-{DateTime.UtcNow.Ticks}";
            }

            // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
            var pageBuilder = PageBuilder.Create(
                name: request.Name,
                title: request.Title,
                content: request.Content,
                authorId: request.AuthorId,
                slug: slug,
                template: request.Template,
                status: ContentStatus.Draft,
                pageType: request.PageType,
                relatedEntityId: request.RelatedEntityId,
                metaTitle: request.MetaTitle,
                metaDescription: request.MetaDescription,
                ogImageUrl: request.OgImageUrl);

            pageBuilder = await _pageBuilderRepository.AddAsync(pageBuilder, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            // ✅ PERFORMANCE: Reload with Include instead of LoadAsync (N+1 fix)
            var reloadedPage = await _context.Set<PageBuilder>()
                .AsNoTracking()
                .Include(pb => pb.Author)
                .FirstOrDefaultAsync(pb => pb.Id == pageBuilder.Id, cancellationToken);

            if (reloadedPage == null)
            {
                _logger.LogWarning("Page builder {PageBuilderId} not found after creation", pageBuilder.Id);
                throw new NotFoundException("Page Builder", pageBuilder.Id);
            }

            // ✅ BOLUM 10.2: Cache invalidation
            await _cache.RemoveAsync(CACHE_KEY_ALL_PAGES, cancellationToken);
            await _cache.RemoveAsync(CACHE_KEY_ACTIVE_PAGES, cancellationToken);
            await _cache.RemoveAsync($"page_builder_{pageBuilder.Id}", cancellationToken);
            await _cache.RemoveAsync($"page_builder_slug_{pageBuilder.Slug}", cancellationToken);

            _logger.LogInformation("Page builder created. PageBuilderId: {PageBuilderId}, Slug: {Slug}, AuthorId: {AuthorId}",
                pageBuilder.Id, pageBuilder.Slug, request.AuthorId);

            return _mapper.Map<PageBuilderDto>(reloadedPage);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            _logger.LogError(ex, "Concurrency conflict while creating page builder with Name: {Name}", request.Name);
            throw new BusinessException("Page builder oluşturma çakışması. Lütfen tekrar deneyin.");
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            _logger.LogError(ex, "Error creating page builder with Name: {Name}", request.Name);
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    private static string GenerateSlug(string name)
    {
        var slug = name.ToLowerInvariant()
            .Replace("ğ", "g")
            .Replace("ü", "u")
            .Replace("ş", "s")
            .Replace("ı", "i")
            .Replace("ö", "o")
            .Replace("ç", "c")
            .Replace(" ", "-")
            .Replace(".", "")
            .Replace(",", "")
            .Replace("!", "")
            .Replace("?", "")
            .Replace(":", "")
            .Replace(";", "");

        while (slug.Contains("--"))
        {
            slug = slug.Replace("--", "-");
        }

        return slug.Trim('-');
    }
}

