using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Microsoft.Extensions.Options;
using Merge.Application.DTOs.Support;
using Merge.Application.Interfaces;
using Merge.Application.Configuration;
using Merge.Domain.Entities;
using Merge.Domain.Enums;

namespace Merge.Application.Support.Commands.CreateKnowledgeBaseArticle;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class CreateKnowledgeBaseArticleCommandHandler : IRequestHandler<CreateKnowledgeBaseArticleCommand, KnowledgeBaseArticleDto>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateKnowledgeBaseArticleCommandHandler> _logger;
    private readonly SupportSettings _settings;

    public CreateKnowledgeBaseArticleCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<CreateKnowledgeBaseArticleCommandHandler> logger,
        IOptions<SupportSettings> settings)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
        _settings = settings.Value;
    }

    public async Task<KnowledgeBaseArticleDto> Handle(CreateKnowledgeBaseArticleCommand request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation("Creating knowledge base article. Title: {Title}, AuthorId: {AuthorId}, Status: {Status}",
            request.Title, request.AuthorId, request.Status);

        var slug = GenerateSlug(request.Title);

        // ✅ PERFORMANCE: Global Query Filter otomatik uygulanır, manuel !IsDeleted kontrolü YASAK
        // Ensure unique slug
        var existingSlug = await _context.Set<KnowledgeBaseArticle>()
            .AsNoTracking()
            .AnyAsync(a => a.Slug == slug, cancellationToken);
        
        if (existingSlug)
        {
            slug = $"{slug}-{DateTime.UtcNow.Ticks}";
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
        var article = KnowledgeBaseArticle.Create(
            request.Title,
            slug,
            request.Content,
            request.AuthorId,
            request.CategoryId,
            request.Excerpt,
            Enum.Parse<ContentStatus>(request.Status, true),
            request.IsFeatured,
            request.DisplayOrder,
            request.Tags != null ? string.Join(",", request.Tags) : null);

        await _context.Set<KnowledgeBaseArticle>().AddAsync(article, cancellationToken);
        
        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Knowledge base article {ArticleId} created successfully. Title: {Title}, Slug: {Slug}",
            article.Id, request.Title, slug);

        // ✅ PERFORMANCE: Reload with includes for mapping
        article = await _context.Set<KnowledgeBaseArticle>()
            .AsNoTracking()
            .Include(a => a.Category)
            .Include(a => a.Author)
            .FirstOrDefaultAsync(a => a.Id == article.Id, cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan
        return _mapper.Map<KnowledgeBaseArticleDto>(article!);
    }

    private string GenerateSlug(string title)
    {
        var slug = title.ToLowerInvariant()
            .Replace(" ", "-")
            .Replace("ğ", "g")
            .Replace("ü", "u")
            .Replace("ş", "s")
            .Replace("ı", "i")
            .Replace("ö", "o")
            .Replace("ç", "c")
            .Replace("Ğ", "g")
            .Replace("Ü", "u")
            .Replace("Ş", "s")
            .Replace("İ", "i")
            .Replace("Ö", "o")
            .Replace("Ç", "c");

        // Remove special characters
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[^a-z0-9\-]", "");
        
        // Remove multiple dashes
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"-+", "-");
        
        // Remove leading/trailing dashes
        slug = slug.Trim('-');

        // ✅ BOLUM 12.0: Magic Number'ları Configuration'a Taşıma
        if (slug.Length > _settings.MaxArticleSlugLength)
        {
            slug = slug.Substring(0, _settings.MaxArticleSlugLength);
        }

        return slug;
    }
}
