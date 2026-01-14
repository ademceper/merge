using MediatR;
using Merge.Application.DTOs.International;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.International.Commands.UpdateStaticTranslation;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record UpdateStaticTranslationCommand(
    Guid Id,
    string Value,
    string Category) : IRequest<StaticTranslationDto>;

