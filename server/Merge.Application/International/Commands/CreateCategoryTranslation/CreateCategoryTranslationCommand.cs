using MediatR;
using Merge.Application.DTOs.International;

namespace Merge.Application.International.Commands.CreateCategoryTranslation;

public record CreateCategoryTranslationCommand(
    Guid CategoryId,
    string LanguageCode,
    string Name,
    string Description) : IRequest<CategoryTranslationDto>;

