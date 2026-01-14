using FluentValidation;
using Merge.Domain.Modules.Identity;

namespace Merge.Application.Identity.Commands.VerifyBackupCode;

// ✅ BOLUM 2.3: FluentValidation (ZORUNLU)
public class VerifyBackupCodeCommandValidator : AbstractValidator<VerifyBackupCodeCommand>
{
    public VerifyBackupCodeCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID zorunludur.");

        RuleFor(x => x.BackupCode)
            .NotEmpty().WithMessage("Backup kod zorunludur.")
            .MaximumLength(20).WithMessage("Backup kod en fazla 20 karakter olabilir.");
    }
}

