using MediatR;
using Merge.Application.DTOs.International;

namespace Merge.Application.International.Commands.PatchLanguage;

/// <summary>
/// PATCH command for partial language updates
/// HIGH-API-001: PATCH Support - Partial updates without requiring all fields
/// </summary>
public record PatchLanguageCommand(
    Guid Id,
    PatchLanguageDto PatchDto
) : IRequest<LanguageDto>;
