using FluentValidation;

namespace Merge.Application.Marketing.Commands.CreateLoyaltyAccount;

// ✅ BOLUM 2.0: FluentValidation (ZORUNLU)
public class CreateLoyaltyAccountCommandValidator : AbstractValidator<CreateLoyaltyAccountCommand>
{
    public CreateLoyaltyAccountCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Kullanıcı ID'si zorunludur.");
    }
}
