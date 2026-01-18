using FluentValidation;

namespace Merge.Application.Logistics.Queries.GetShippingById;

public class GetShippingByIdQueryValidator : AbstractValidator<GetShippingByIdQuery>
{
    public GetShippingByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Kargo ID'si zorunludur.");
    }
}

