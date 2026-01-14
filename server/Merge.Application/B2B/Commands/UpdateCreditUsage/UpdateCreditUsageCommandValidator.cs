using FluentValidation;

namespace Merge.Application.B2B.Commands.UpdateCreditUsage;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class UpdateCreditUsageCommandValidator : AbstractValidator<UpdateCreditUsageCommand>
{
    public UpdateCreditUsageCommandValidator()
    {
        RuleFor(x => x.CreditTermId)
            .NotEmpty().WithMessage("Kredi koşulu ID boş olamaz");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Miktar 0'dan büyük olmalıdır");
    }
}

