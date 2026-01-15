using MediatR;
using Merge.Application.DTOs.International;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.International.Commands.UpdateStaticTranslation;

public record UpdateStaticTranslationCommand(
    Guid Id,
    string Value,
    string Category) : IRequest<StaticTranslationDto>;

