using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Content;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Application.Content.Commands.CreateOrUpdateSEOSettings;

namespace Merge.Application.Content.Commands.GenerateCategorySEO;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class GenerateCategorySEOCommandHandler : IRequestHandler<GenerateCategorySEOCommand, SEOSettingsDto>
{
    private readonly IDbContext _context;
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;
    private readonly ILogger<GenerateCategorySEOCommandHandler> _logger;

    public GenerateCategorySEOCommandHandler(
        IDbContext context,
        IMediator mediator,
        IMapper mapper,
        ILogger<GenerateCategorySEOCommandHandler> logger)
    {
        _context = context;
        _mediator = mediator;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<SEOSettingsDto> Handle(GenerateCategorySEOCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Generating SEO for category. CategoryId: {CategoryId}", request.CategoryId);

        // ✅ PERFORMANCE: Removed manual !c.IsDeleted (Global Query Filter)
        var category = await _context.Set<Category>()
            .FirstOrDefaultAsync(c => c.Id == request.CategoryId, cancellationToken);

        if (category == null)
        {
            _logger.LogWarning("Category not found. CategoryId: {CategoryId}", request.CategoryId);
            throw new NotFoundException("Kategori", request.CategoryId);
        }

        var metaTitle = $"{category.Name} - Shop Online";
        var metaDescription = !string.IsNullOrEmpty(category.Description)
            ? category.Description.Length > 160
                ? category.Description.Substring(0, 157) + "..."
                : category.Description
            : $"Browse {category.Name} products. Wide selection and best prices.";

        // ✅ BOLUM 2.0: MediatR + CQRS pattern - CreateOrUpdateSEOSettingsCommand kullan
        var command = new CreateOrUpdateSEOSettingsCommand(
            PageType: "Category",
            EntityId: request.CategoryId,
            MetaTitle: metaTitle,
            MetaDescription: metaDescription,
            MetaKeywords: category.Name,
            CanonicalUrl: $"/categories/{category.Slug}",
            IsIndexed: true,
            FollowLinks: true,
            Priority: 0.7m,
            ChangeFrequency: "daily");

        return await _mediator.Send(command, cancellationToken);
    }
}

