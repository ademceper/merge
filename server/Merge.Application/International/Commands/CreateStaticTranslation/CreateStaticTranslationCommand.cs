using MediatR;
using Merge.Application.DTOs.International;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.International.Commands.CreateStaticTranslation;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record CreateStaticTranslationCommand(
    string Key,
    string LanguageCode,
    string Value,
    string Category) : IRequest<StaticTranslationDto>;

