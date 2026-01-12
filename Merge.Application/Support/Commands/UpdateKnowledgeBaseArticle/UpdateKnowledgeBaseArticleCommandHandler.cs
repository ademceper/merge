using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Microsoft.Extensions.Options;
using Merge.Application.DTOs.Support;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Application.Configuration;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Content;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Support.Commands.UpdateKnowledgeBaseArticle;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class UpdateKnowledgeBaseArticleCommandHandler : IRequestHandler<UpdateKnowledgeBaseArticleCommand, KnowledgeBaseArticleDto?>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<UpdateKnowledgeBaseArticleCommandHandler> _logger;
    private readonly SupportSettings _settings;

    public UpdateKnowledgeBaseArticleCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<UpdateKnowledgeBaseArticleCommandHandler> logger,
        IOptions<SupportSettings> settings)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
        _settings = settings.Value;
    }

    public async Task<KnowledgeBaseArticleDto?> Handle(UpdateKnowledgeBaseArticleCommand request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation("Updating knowledge base article {ArticleId}", request.ArticleId);

        var article = await _context.Set<KnowledgeBaseArticle>()
            .FirstOrDefaultAsync(a => a.Id == request.ArticleId, cancellationToken);

        if (article == null)
        {
            _logger.LogWarning("Knowledge base article {ArticleId} not found for update", request.ArticleId);
            throw new NotFoundException("Bilgi bankası makalesi", request.ArticleId);
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        if (!string.IsNullOrEmpty(request.Title))
        {
            var newSlug = GenerateSlug(request.Title);
            article.UpdateTitle(request.Title, newSlug);
        }
        if (!string.IsNullOrEmpty(request.Content))
        {
            article.UpdateContent(request.Content, request.Excerpt);
        }
        else if (request.Excerpt != null)
        {
            article.UpdateContent(article.Content, request.Excerpt);
        }
        if (request.CategoryId.HasValue)
        {
            article.UpdateCategory(request.CategoryId.Value);
        }
        if (!string.IsNullOrEmpty(request.Status))
        {
            var newStatus = Enum.Parse<ContentStatus>(request.Status, true);
            if (newStatus == ContentStatus.Published && article.Status != ContentStatus.Published)
            {
                article.Publish();
            }
            else
            {
                article.UpdateStatus(newStatus);
            }
        }
        if (request.IsFeatured.HasValue)
        {
            article.SetFeatured(request.IsFeatured.Value);
        }
        if (request.DisplayOrder.HasValue)
        {
            article.UpdateDisplayOrder(request.DisplayOrder.Value);
        }
        if (request.Tags != null)
        {
            var tagsString = string.Join(",", request.Tags);
            article.UpdateTags(tagsString);
        }

        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Knowledge base article {ArticleId} updated successfully", request.ArticleId);

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

        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[^a-z0-9\-]", "");
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"-+", "-");
        slug = slug.Trim('-');

        if (slug.Length > _settings.MaxArticleSlugLength)
        {
            slug = slug.Substring(0, _settings.MaxArticleSlugLength);
        }

        return slug;
    }
}
