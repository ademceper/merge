using FluentValidation;
using Merge.Application.DTOs.B2B;

namespace Merge.Application.B2B.Commands.CreateCreditTerm;

public class CreateCreditTermCommandValidator : AbstractValidator<CreateCreditTermCommand>
{
    public CreateCreditTermCommandValidator()
    {
        RuleFor(x => x.Dto)
            .NotNull().WithMessage("Kredi koşulu verisi boş olamaz");

        RuleFor(x => x.Dto.OrganizationId)
            .NotEmpty().WithMessage("Organizasyon ID boş olamaz");

        RuleFor(x => x.Dto.Name)
            .NotEmpty().WithMessage("Kredi koşulu adı boş olamaz")
            .MaximumLength(200).WithMessage("Kredi koşulu adı en fazla 200 karakter olabilir");

        RuleFor(x => x.Dto.PaymentDays)
            .GreaterThan(0).WithMessage("Ödeme günü 0'dan büyük olmalıdır");
    }
}

