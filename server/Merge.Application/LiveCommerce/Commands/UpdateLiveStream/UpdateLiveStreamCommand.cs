using MediatR;
using Merge.Application.DTOs.LiveCommerce;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.LiveCommerce.Commands.UpdateLiveStream;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record UpdateLiveStreamCommand(
    Guid StreamId,
    string Title,
    string Description,
    DateTime? ScheduledStartTime,
    string? StreamUrl,
    string? StreamKey,
    string? ThumbnailUrl,
    string? Category,
    string? Tags) : IRequest<LiveStreamDto>;

