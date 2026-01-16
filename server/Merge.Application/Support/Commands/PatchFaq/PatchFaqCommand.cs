using MediatR;
using Merge.Application.DTOs.Support;

namespace Merge.Application.Support.Commands.PatchFaq;

/// <summary>
/// PATCH command for partial FAQ updates
/// HIGH-API-001: PATCH Support - Partial updates without requiring all fields
/// </summary>
public record PatchFaqCommand(
    Guid FaqId,
    PatchFaqDto PatchDto
) : IRequest<FaqDto?>;
