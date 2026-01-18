using FluentValidation;

namespace Merge.Application.Logistics.Queries.GetPickPackById;

public class GetPickPackByIdQueryValidator : AbstractValidator<GetPickPackByIdQuery>
{
    public GetPickPackByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Pick-pack ID'si zorunludur.");
    }
}

