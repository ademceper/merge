using FluentValidation;
using Merge.Domain.Modules.Identity;

namespace Merge.Application.Identity.Commands.SendVerificationCode;

public class SendVerificationCodeCommandValidator : AbstractValidator<SendVerificationCodeCommand>
{
    public SendVerificationCodeCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID zorunludur.");

        RuleFor(x => x.Purpose)
            .NotEmpty().WithMessage("Purpose zorunludur.")
            .MaximumLength(50).WithMessage("Purpose en fazla 50 karakter olabilir.");
    }
}

