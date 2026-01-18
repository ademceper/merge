using FluentValidation;

namespace Merge.Application.Logistics.Queries.GetWarehouseByCode;

public class GetWarehouseByCodeQueryValidator : AbstractValidator<GetWarehouseByCodeQuery>
{
    public GetWarehouseByCodeQueryValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Depo kodu zorunludur.")
            .MaximumLength(50).WithMessage("Depo kodu en fazla 50 karakter olabilir.");
    }
}

