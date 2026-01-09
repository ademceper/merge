using MediatR;
using Merge.Application.DTOs.International;

namespace Merge.Application.International.Commands.UpdateCategoryTranslation;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record UpdateCategoryTranslationCommand(
    Guid Id,
    string Name,
    string Description) : IRequest<CategoryTranslationDto>;

