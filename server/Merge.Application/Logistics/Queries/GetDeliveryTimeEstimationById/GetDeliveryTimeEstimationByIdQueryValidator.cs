using FluentValidation;

namespace Merge.Application.Logistics.Queries.GetDeliveryTimeEstimationById;

public class GetDeliveryTimeEstimationByIdQueryValidator : AbstractValidator<GetDeliveryTimeEstimationByIdQuery>
{
    public GetDeliveryTimeEstimationByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Teslimat süresi tahmini ID'si zorunludur.");
    }
}

