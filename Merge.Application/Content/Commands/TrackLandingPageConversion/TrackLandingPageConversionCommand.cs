using MediatR;

namespace Merge.Application.Content.Commands.TrackLandingPageConversion;

public record TrackLandingPageConversionCommand(
    Guid Id
) : IRequest<bool>;

