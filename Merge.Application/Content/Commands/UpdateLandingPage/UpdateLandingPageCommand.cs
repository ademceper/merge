using MediatR;

namespace Merge.Application.Content.Commands.UpdateLandingPage;

public record UpdateLandingPageCommand(
    Guid Id,
    string? Name = null,
    string? Title = null,
    string? Content = null,
    string? Template = null,
    string? Status = null, // Enum string olarak alınıp handler'da parse edilecek
    DateTime? StartDate = null,
    DateTime? EndDate = null,
    string? MetaTitle = null,
    string? MetaDescription = null,
    string? OgImageUrl = null,
    bool? EnableABTesting = null,
    int? TrafficSplit = null,
    Guid? PerformedBy = null // IDOR protection için
) : IRequest<bool>;

