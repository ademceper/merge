using FluentValidation;

namespace Merge.Application.Logistics.Queries.GetPickPackById;

// ✅ BOLUM 2.0: FluentValidation (ZORUNLU)
public class GetPickPackByIdQueryValidator : AbstractValidator<GetPickPackByIdQuery>
{
    public GetPickPackByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Pick-pack ID'si zorunludur.");
    }
}

