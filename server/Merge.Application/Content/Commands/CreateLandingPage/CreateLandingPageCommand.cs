using MediatR;
using Merge.Application.DTOs.Content;

namespace Merge.Application.Content.Commands.CreateLandingPage;

public record CreateLandingPageCommand(
    Guid? AuthorId, // Controller'dan set edilecek
    string Name,
    string Title,
    string Content,
    string? Template = null,
    string Status = "Draft", // Enum string olarak alınıp handler'da parse edilecek
    DateTime? StartDate = null,
    DateTime? EndDate = null,
    string? MetaTitle = null,
    string? MetaDescription = null,
    string? OgImageUrl = null,
    bool EnableABTesting = false,
    Guid? VariantOfId = null,
    int TrafficSplit = 50
) : IRequest<LandingPageDto>;

