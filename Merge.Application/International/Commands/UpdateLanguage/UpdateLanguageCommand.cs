using MediatR;
using Merge.Application.DTOs.International;

namespace Merge.Application.International.Commands.UpdateLanguage;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record UpdateLanguageCommand(
    Guid Id,
    string Name,
    string NativeName,
    bool IsActive,
    bool IsRTL,
    string FlagIcon) : IRequest<LanguageDto>;

