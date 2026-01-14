using MediatR;
using Merge.Application.DTOs.International;

namespace Merge.Application.International.Commands.UpdateProductTranslation;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record UpdateProductTranslationCommand(
    Guid Id,
    string Name,
    string Description,
    string ShortDescription,
    string MetaTitle,
    string MetaDescription,
    string MetaKeywords) : IRequest<ProductTranslationDto>;

