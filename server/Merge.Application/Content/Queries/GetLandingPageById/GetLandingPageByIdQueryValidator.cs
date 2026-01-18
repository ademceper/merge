using FluentValidation;

namespace Merge.Application.Content.Queries.GetLandingPageById;

public class GetLandingPageByIdQueryValidator : AbstractValidator<GetLandingPageByIdQuery>
{
    public GetLandingPageByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("ID gereklidir");
    }
}

