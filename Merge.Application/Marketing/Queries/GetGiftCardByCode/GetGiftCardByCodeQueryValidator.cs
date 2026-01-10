using FluentValidation;

namespace Merge.Application.Marketing.Queries.GetGiftCardByCode;

public class GetGiftCardByCodeQueryValidator : AbstractValidator<GetGiftCardByCodeQuery>
{
    public GetGiftCardByCodeQueryValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Hediye kartı kodu zorunludur.")
            .MaximumLength(50).WithMessage("Hediye kartı kodu en fazla 50 karakter olabilir.");
    }
}
