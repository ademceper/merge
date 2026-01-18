using FluentValidation;

namespace Merge.Application.Logistics.Queries.GetPickPackByPackNumber;

public class GetPickPackByPackNumberQueryValidator : AbstractValidator<GetPickPackByPackNumberQuery>
{
    public GetPickPackByPackNumberQueryValidator()
    {
        RuleFor(x => x.PackNumber)
            .NotEmpty().WithMessage("Paket numarası zorunludur.")
            .MaximumLength(50).WithMessage("Paket numarası en fazla 50 karakter olabilir.");
    }
}

