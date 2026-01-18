using FluentValidation;

namespace Merge.Application.Marketing.Commands.CreateReferralCode;

public class CreateReferralCodeCommandValidator : AbstractValidator<CreateReferralCodeCommand>
{
    public CreateReferralCodeCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Kullanıcı ID'si zorunludur.");
    }
}
