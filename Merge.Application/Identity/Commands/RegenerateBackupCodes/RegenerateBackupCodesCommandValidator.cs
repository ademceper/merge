using FluentValidation;

namespace Merge.Application.Identity.Commands.RegenerateBackupCodes;

// ✅ BOLUM 2.3: FluentValidation (ZORUNLU)
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

