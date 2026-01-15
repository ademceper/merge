using FluentValidation;

namespace Merge.Application.International.Queries.GetLanguageById;

public class GetLanguageByIdQueryValidator : AbstractValidator<GetLanguageByIdQuery>
{
    public GetLanguageByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Dil ID'si zorunludur.");
    }
}

