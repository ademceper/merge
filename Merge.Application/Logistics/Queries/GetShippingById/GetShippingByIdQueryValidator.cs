using FluentValidation;

namespace Merge.Application.Logistics.Queries.GetShippingById;

// ✅ BOLUM 2.0: FluentValidation (ZORUNLU)
public class GetShippingByIdQueryValidator : AbstractValidator<GetShippingByIdQuery>
{
    public GetShippingByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Kargo ID'si zorunludur.");
    }
}

