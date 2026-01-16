using MediatR;
using Merge.Application.DTOs.LiveCommerce;

namespace Merge.Application.LiveCommerce.Commands.PatchLiveStream;

/// <summary>
/// PATCH command for partial live stream updates
/// HIGH-API-001: PATCH Support - Partial updates without requiring all fields
/// </summary>
public record PatchLiveStreamCommand(
    Guid StreamId,
    PatchLiveStreamDto PatchDto
) : IRequest<LiveStreamDto>;
