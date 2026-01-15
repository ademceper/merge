using MediatR;
using Merge.Application.DTOs.International;

namespace Merge.Application.International.Commands.UpdateCategoryTranslation;

public record UpdateCategoryTranslationCommand(
    Guid Id,
    string Name,
    string Description) : IRequest<CategoryTranslationDto>;

