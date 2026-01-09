using MediatR;
using Merge.Application.DTOs.Content;

namespace Merge.Application.Content.Commands.CreateLandingPageVariant;

public record CreateLandingPageVariantCommand(
    Guid OriginalId,
    string Name,
    string Title,
    string Content,
    string? Template = null,
    string Status = "Draft",
    DateTime? StartDate = null,
    DateTime? EndDate = null,
    string? MetaTitle = null,
    string? MetaDescription = null,
    string? OgImageUrl = null,
    int TrafficSplit = 50
) : IRequest<LandingPageDto>;

