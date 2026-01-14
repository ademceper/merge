using MediatR;
using Merge.Application.DTOs.Content;

namespace Merge.Application.Content.Commands.UpdateBlogPost;

public record UpdateBlogPostCommand(
    Guid Id,
    Guid? CategoryId = null,
    string? Title = null,
    string? Excerpt = null,
    string? Content = null,
    string? FeaturedImageUrl = null,
    string? Status = null, // Enum string olarak alınıp handler'da parse edilecek
    List<string>? Tags = null,
    bool? IsFeatured = null,
    bool? AllowComments = null,
    string? MetaTitle = null,
    string? MetaDescription = null,
    string? MetaKeywords = null,
    string? OgImageUrl = null,
    Guid? PerformedBy = null // IDOR protection için
) : IRequest<bool>;

