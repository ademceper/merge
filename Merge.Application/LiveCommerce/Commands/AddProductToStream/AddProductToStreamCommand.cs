using MediatR;
using Merge.Application.DTOs.LiveCommerce;

namespace Merge.Application.LiveCommerce.Commands.AddProductToStream;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record AddProductToStreamCommand(
    Guid StreamId,
    Guid ProductId,
    int DisplayOrder,
    decimal? SpecialPrice,
    string? ShowcaseNotes) : IRequest<LiveStreamDto>;

