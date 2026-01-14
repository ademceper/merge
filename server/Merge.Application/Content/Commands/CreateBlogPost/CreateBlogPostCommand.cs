using MediatR;
using Merge.Application.DTOs.Content;

namespace Merge.Application.Content.Commands.CreateBlogPost;

public record CreateBlogPostCommand(
    Guid CategoryId,
    Guid AuthorId, // Controller'dan set edilecek
    string Title,
    string Excerpt,
    string Content,
    string? FeaturedImageUrl = null,
    string Status = "Draft", // Enum string olarak alınıp handler'da parse edilecek
    List<string>? Tags = null,
    bool IsFeatured = false,
    bool AllowComments = true,
    string? MetaTitle = null,
    string? MetaDescription = null,
    string? MetaKeywords = null,
    string? OgImageUrl = null
) : IRequest<BlogPostDto>;

