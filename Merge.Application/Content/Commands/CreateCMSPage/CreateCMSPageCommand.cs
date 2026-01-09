using MediatR;
using Merge.Application.DTOs.Content;

namespace Merge.Application.Content.Commands.CreateCMSPage;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record CreateCMSPageCommand(
    Guid? AuthorId,
    string Title,
    string Content,
    string? Excerpt = null,
    string PageType = "Page",
    string Status = "Draft",
    string? Template = null,
    string? MetaTitle = null,
    string? MetaDescription = null,
    string? MetaKeywords = null,
    bool IsHomePage = false,
    int DisplayOrder = 0,
    bool ShowInMenu = true,
    string? MenuTitle = null,
    Guid? ParentPageId = null
) : IRequest<CMSPageDto>;

