using FluentValidation;

namespace Merge.Application.Logistics.Queries.GetWarehouseById;

// ✅ BOLUM 2.0: FluentValidation (ZORUNLU)
public class GetWarehouseByIdQueryValidator : AbstractValidator<GetWarehouseByIdQuery>
{
    public GetWarehouseByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Depo ID'si zorunludur.");
    }
}

