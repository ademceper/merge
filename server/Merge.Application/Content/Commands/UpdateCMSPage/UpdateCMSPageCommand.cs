using MediatR;
using Merge.Application.DTOs.Content;

namespace Merge.Application.Content.Commands.UpdateCMSPage;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record UpdateCMSPageCommand(
    Guid Id,
    Guid? PerformedBy = null, // IDOR protection için
    string? Title = null,
    string? Content = null,
    string? Excerpt = null,
    string? PageType = null,
    string? Status = null,
    string? Template = null,
    string? MetaTitle = null,
    string? MetaDescription = null,
    string? MetaKeywords = null,
    bool? IsHomePage = null,
    int? DisplayOrder = null,
    bool? ShowInMenu = null,
    string? MenuTitle = null,
    Guid? ParentPageId = null
) : IRequest<bool>;

