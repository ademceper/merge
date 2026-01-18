using FluentValidation;

namespace Merge.Application.Marketing.Commands.CreateLoyaltyAccount;

public class CreateLoyaltyAccountCommandValidator : AbstractValidator<CreateLoyaltyAccountCommand>
{
    public CreateLoyaltyAccountCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Kullanıcı ID'si zorunludur.");
    }
}
