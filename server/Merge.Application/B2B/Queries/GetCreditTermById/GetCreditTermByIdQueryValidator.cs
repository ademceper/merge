using FluentValidation;

namespace Merge.Application.B2B.Queries.GetCreditTermById;

public class GetCreditTermByIdQueryValidator : AbstractValidator<GetCreditTermByIdQuery>
{
    public GetCreditTermByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Kredi koşulu ID boş olamaz");
    }
}

