using MediatR;
using Merge.Application.DTOs.Content;

namespace Merge.Application.Content.Commands.CreatePageBuilder;

public record CreatePageBuilderCommand(
    Guid? AuthorId, // Controller'dan set edilecek
    string Name,
    string Title,
    string Content,
    string? Slug = null,
    string? Template = null,
    string? PageType = null,
    Guid? RelatedEntityId = null,
    string? MetaTitle = null,
    string? MetaDescription = null,
    string? OgImageUrl = null
) : IRequest<PageBuilderDto>;

