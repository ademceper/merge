using MediatR;
using Merge.Application.DTOs.International;

namespace Merge.Application.International.Commands.CreateCategoryTranslation;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record CreateCategoryTranslationCommand(
    Guid CategoryId,
    string LanguageCode,
    string Name,
    string Description) : IRequest<CategoryTranslationDto>;

