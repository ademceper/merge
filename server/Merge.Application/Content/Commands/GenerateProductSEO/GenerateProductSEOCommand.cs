using MediatR;
using Merge.Application.DTOs.Content;

namespace Merge.Application.Content.Commands.GenerateProductSEO;

public record GenerateProductSEOCommand(
    Guid ProductId
) : IRequest<SEOSettingsDto>;

