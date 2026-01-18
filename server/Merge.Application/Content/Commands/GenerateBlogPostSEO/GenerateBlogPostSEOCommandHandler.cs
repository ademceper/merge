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
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Content;
using Merge.Domain.ValueObjects;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Content.Commands.GenerateBlogPostSEO;

public class GenerateBlogPostSEOCommandHandler(
    IDbContext context,
    IMediator mediator,
    IMapper mapper,
    ILogger<GenerateBlogPostSEOCommandHandler> logger) : IRequestHandler<GenerateBlogPostSEOCommand, SEOSettingsDto>
{

    public async Task<SEOSettingsDto> Handle(GenerateBlogPostSEOCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Generating SEO for blog post. PostId: {PostId}", request.PostId);

        var post = await context.Set<BlogPost>()
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == request.PostId, cancellationToken);

        if (post is null)
        {
            logger.LogWarning("Blog post not found. PostId: {PostId}", request.PostId);
            throw new NotFoundException("Blog yazısı", request.PostId);
        }

        var metaTitle = post.MetaTitle ?? post.Title;
        var metaDescription = post.MetaDescription ?? post.Excerpt;

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

        return await mediator.Send(command, cancellationToken);
    }
}

