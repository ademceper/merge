using FluentValidation;

namespace Merge.Application.User.Queries.GetActivityById;

// ✅ BOLUM 2.0: FluentValidation (ZORUNLU)
public class GetActivityByIdQueryValidator : AbstractValidator<GetActivityByIdQuery>
{
    public GetActivityByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Aktivite ID'si zorunludur.");
    }
}
