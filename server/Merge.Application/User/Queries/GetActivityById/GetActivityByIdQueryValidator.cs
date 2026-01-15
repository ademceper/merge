using FluentValidation;

namespace Merge.Application.User.Queries.GetActivityById;

public class GetActivityByIdQueryValidator : AbstractValidator<GetActivityByIdQuery>
{
    public GetActivityByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Aktivite ID'si zorunludur.");
    }
}
