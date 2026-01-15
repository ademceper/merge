using MediatR;
using Merge.Application.DTOs.International;

namespace Merge.Application.International.Commands.UpdateLanguage;

public record UpdateLanguageCommand(
    Guid Id,
    string Name,
    string NativeName,
    bool IsActive,
    bool IsRTL,
    string FlagIcon) : IRequest<LanguageDto>;

