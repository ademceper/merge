using FluentValidation;
using Merge.Domain.Modules.Identity;

namespace Merge.Application.Identity.Commands.RegenerateBackupCodes;

public class RegenerateBackupCodesCommandValidator : AbstractValidator<RegenerateBackupCodesCommand>
{
    public RegenerateBackupCodesCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID zorunludur.");

        RuleFor(x => x.RegenerateDto)
            .NotNull().WithMessage("Regenerate DTO zorunludur.");
    }
}

