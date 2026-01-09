using MediatR;
using Merge.Application.DTOs.Content;

namespace Merge.Application.Content.Commands.CreateOrUpdateSEOSettings;

public record CreateOrUpdateSEOSettingsCommand(
    string PageType,
    Guid? EntityId = null,
    string? MetaTitle = null,
    string? MetaDescription = null,
    string? MetaKeywords = null,
    string? CanonicalUrl = null,
    string? OgTitle = null,
    string? OgDescription = null,
    string? OgImageUrl = null,
    string? TwitterCard = null,
    string? StructuredDataJson = null,
    bool IsIndexed = true,
    bool FollowLinks = true,
    decimal Priority = 0.5m,
    string? ChangeFrequency = null
) : IRequest<SEOSettingsDto>;

