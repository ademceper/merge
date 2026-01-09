using MediatR;
using Merge.Application.DTOs.Content;

namespace Merge.Application.Content.Commands.GenerateCategorySEO;

public record GenerateCategorySEOCommand(
    Guid CategoryId
) : IRequest<SEOSettingsDto>;

