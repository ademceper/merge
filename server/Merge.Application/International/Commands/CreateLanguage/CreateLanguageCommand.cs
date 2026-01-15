using MediatR;
using Merge.Application.DTOs.International;

namespace Merge.Application.International.Commands.CreateLanguage;

public record CreateLanguageCommand(
    string Code,
    string Name,
    string NativeName,
    bool IsDefault,
    bool IsActive,
    bool IsRTL,
    string FlagIcon) : IRequest<LanguageDto>;

