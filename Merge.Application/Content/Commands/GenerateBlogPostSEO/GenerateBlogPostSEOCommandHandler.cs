using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Content;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Application.Content.Commands.CreateOrUpdateSEOSettings;

namespace Merge.Application.Content.Commands.GenerateBlogPostSEO;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class GenerateBlogPostSEOCommandHandler : IRequestHandler<GenerateBlogPostSEOCommand, SEOSettingsDto>
{
    private readonly IDbContext _context;
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;
    private readonly ILogger<GenerateBlogPostSEOCommandHandler> _logger;

    public GenerateBlogPostSEOCommandHandler(
        IDbContext context,
        IMediator mediator,
        IMapper mapper,
        ILogger<GenerateBlogPostSEOCommandHandler> logger)
    {
        _context = context;
        _mediator = mediator;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<SEOSettingsDto> Handle(GenerateBlogPostSEOCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Generating SEO for blog post. PostId: {PostId}", request.PostId);

        // ✅ PERFORMANCE: Removed manual !p.IsDeleted (Global Query Filter)
        var post = await _context.Set<BlogPost>()
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == request.PostId, cancellationToken);

        if (post == null)
        {
            _logger.LogWarning("Blog post not found. PostId: {PostId}", request.PostId);
            throw new NotFoundException("Blog yazısı", request.PostId);
        }

        var metaTitle = post.MetaTitle ?? post.Title;
        var metaDescription = post.MetaDescription ?? post.Excerpt;

        // ✅ BOLUM 2.0: MediatR + CQRS pattern - CreateOrUpdateSEOSettingsCommand kullan
        var command = new CreateOrUpdateSEOSettingsCommand(
            PageType: "Blog",
            EntityId: request.PostId,
            MetaTitle: metaTitle,
            MetaDescription: metaDescription,
            MetaKeywords: post.MetaKeywords ?? post.Tags,
            CanonicalUrl: $"/blog/{post.Slug}",
            OgTitle: metaTitle,
            OgDescription: metaDescription,
            OgImageUrl: post.OgImageUrl ?? post.FeaturedImageUrl,
            IsIndexed: post.Status == ContentStatus.Published,
            FollowLinks: true,
            Priority: 0.6m,
            ChangeFrequency: "weekly");

        return await _mediator.Send(command, cancellationToken);
    }
}

