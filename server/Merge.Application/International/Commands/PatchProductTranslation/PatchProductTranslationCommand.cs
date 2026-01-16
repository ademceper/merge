using MediatR;
using Merge.Application.DTOs.International;

namespace Merge.Application.International.Commands.PatchProductTranslation;

public record PatchProductTranslationCommand(
    Guid Id,
    PatchProductTranslationDto PatchDto
) : IRequest<ProductTranslationDto>;
