using MediatR;
using Merge.Application.DTOs.LiveCommerce;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.LiveCommerce.Commands.CreateLiveStream;

public record CreateLiveStreamCommand(
    Guid SellerId,
    string Title,
    string Description,
    DateTime? ScheduledStartTime,
    string? StreamUrl,
    string? StreamKey,
    string? ThumbnailUrl,
    string? Category,
    string? Tags) : IRequest<LiveStreamDto>;
