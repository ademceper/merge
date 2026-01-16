using MediatR;
using Merge.Application.DTOs.International;

namespace Merge.Application.International.Commands.PatchStaticTranslation;

public record PatchStaticTranslationCommand(
    Guid Id,
    PatchStaticTranslationDto PatchDto
) : IRequest<StaticTranslationDto>;
