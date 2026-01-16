using MediatR;
using Merge.Application.DTOs.Content;
using Merge.Application.DTOs.Marketing;

namespace Merge.Application.Content.Commands.PatchBanner;

/// <summary>
/// PATCH command for partial banner updates
/// HIGH-API-001: PATCH Support - Partial updates without requiring all fields
/// </summary>
public record PatchBannerCommand(
    Guid Id,
    PatchBannerDto PatchDto
) : IRequest<BannerDto>;
