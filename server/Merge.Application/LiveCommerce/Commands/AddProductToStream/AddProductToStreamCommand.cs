using MediatR;
using Merge.Application.DTOs.LiveCommerce;

namespace Merge.Application.LiveCommerce.Commands.AddProductToStream;

public record AddProductToStreamCommand(
    Guid StreamId,
    Guid ProductId,
    int DisplayOrder,
    decimal? SpecialPrice,
    string? ShowcaseNotes) : IRequest<LiveStreamDto>;
