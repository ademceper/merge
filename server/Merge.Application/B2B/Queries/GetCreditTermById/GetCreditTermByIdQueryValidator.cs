using FluentValidation;

namespace Merge.Application.B2B.Queries.GetCreditTermById;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class GetCreditTermByIdQueryValidator : AbstractValidator<GetCreditTermByIdQuery>
{
    public GetCreditTermByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Kredi koşulu ID boş olamaz");
    }
}

