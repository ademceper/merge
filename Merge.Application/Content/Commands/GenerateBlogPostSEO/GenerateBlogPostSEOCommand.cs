using MediatR;
using Merge.Application.DTOs.Content;

namespace Merge.Application.Content.Commands.GenerateBlogPostSEO;

public record GenerateBlogPostSEOCommand(
    Guid PostId
) : IRequest<SEOSettingsDto>;

