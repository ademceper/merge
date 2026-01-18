using FluentValidation;

namespace Merge.Application.B2B.Commands.UpdateCreditTerm;

public class UpdateCreditTermCommandValidator : AbstractValidator<UpdateCreditTermCommand>
{
    public UpdateCreditTermCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Kredi koşulu ID boş olamaz");

        RuleFor(x => x.Dto)
            .NotNull().WithMessage("Güncelleme verisi boş olamaz");
    }
}

