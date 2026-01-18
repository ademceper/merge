using FluentValidation;

namespace Merge.Application.Marketing.Queries.GetGiftCardById;

public class GetGiftCardByIdQueryValidator : AbstractValidator<GetGiftCardByIdQuery>
{
    public GetGiftCardByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Hediye kartı ID'si zorunludur.");
    }
}
