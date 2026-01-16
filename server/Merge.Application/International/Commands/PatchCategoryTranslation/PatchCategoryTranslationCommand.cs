using MediatR;
using Merge.Application.DTOs.International;

namespace Merge.Application.International.Commands.PatchCategoryTranslation;

public record PatchCategoryTranslationCommand(
    Guid Id,
    PatchCategoryTranslationDto PatchDto
) : IRequest<CategoryTranslationDto>;
