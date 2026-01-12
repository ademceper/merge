using MediatR;
using Merge.Domain.ValueObjects;

namespace Merge.Application.Content.Commands.UpdatePageBuilder;

public record UpdatePageBuilderCommand(
    Guid Id,
    string? Name = null,
    string? Slug = null,
    string? Title = null,
    string? Content = null,
    string? Template = null,
    string? PageType = null,
    Guid? RelatedEntityId = null,
    string? MetaTitle = null,
    string? MetaDescription = null,
    string? OgImageUrl = null,
    Guid? PerformedBy = null // IDOR protection için
) : IRequest<bool>;

