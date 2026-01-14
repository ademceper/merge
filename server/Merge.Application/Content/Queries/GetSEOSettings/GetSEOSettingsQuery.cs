using MediatR;
using Merge.Application.DTOs.Content;

namespace Merge.Application.Content.Queries.GetSEOSettings;

public record GetSEOSettingsQuery(
    string PageType,
    Guid? EntityId = null
) : IRequest<SEOSettingsDto?>;

