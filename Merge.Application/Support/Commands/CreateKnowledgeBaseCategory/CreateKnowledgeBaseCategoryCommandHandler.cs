using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using AutoMapper;
using Merge.Application.DTOs.Support;
using Merge.Application.Interfaces;
using Merge.Application.Configuration;
using Merge.Domain.Entities;

namespace Merge.Application.Support.Commands.CreateKnowledgeBaseCategory;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class CreateKnowledgeBaseCategoryCommandHandler : IRequestHandler<CreateKnowledgeBaseCategoryCommand, KnowledgeBaseCategoryDto>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateKnowledgeBaseCategoryCommandHandler> _logger;
    private readonly SupportSettings _settings;

    public CreateKnowledgeBaseCategoryCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<CreateKnowledgeBaseCategoryCommandHandler> logger,
        IOptions<SupportSettings> settings)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
        _settings = settings.Value;
    }

    public async Task<KnowledgeBaseCategoryDto> Handle(CreateKnowledgeBaseCategoryCommand request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation("Creating knowledge base category. Name: {Name}, ParentCategoryId: {ParentCategoryId}",
            request.Name, request.ParentCategoryId);

        var slug = GenerateSlug(request.Name);

        // ✅ PERFORMANCE: Global Query Filter otomatik uygulanır, manuel !IsDeleted kontrolü YASAK
        // Ensure unique slug
        var existingSlug = await _context.Set<KnowledgeBaseCategory>()
            .AsNoTracking()
            .AnyAsync(c => c.Slug == slug, cancellationToken);
        
        if (existingSlug)
        {
            slug = $"{slug}-{DateTime.UtcNow.Ticks}";
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
        var category = KnowledgeBaseCategory.Create(
            request.Name,
            slug,
            request.Description,
            request.ParentCategoryId,
            request.DisplayOrder,
            request.IsActive,
            request.IconUrl);

        await _context.Set<KnowledgeBaseCategory>().AddAsync(category, cancellationToken);
        
        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Knowledge base category {CategoryId} created successfully. Name: {Name}, Slug: {Slug}",
            category.Id, request.Name, slug);

        // ✅ PERFORMANCE: Reload with includes for mapping
        category = await _context.Set<KnowledgeBaseCategory>()
            .AsNoTracking()
            .Include(c => c.ParentCategory)
            .FirstOrDefaultAsync(c => c.Id == category.Id, cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan
        return _mapper.Map<KnowledgeBaseCategoryDto>(category!);
    }

    private string GenerateSlug(string name)
    {
        var slug = name.ToLowerInvariant()
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

        // ✅ BOLUM 12.0: Magic Number'ları Configuration'a Taşıma
        if (slug.Length > _settings.MaxCategorySlugLength)
        {
            slug = slug.Substring(0, _settings.MaxCategorySlugLength);
        }

        return slug;
    }
}
