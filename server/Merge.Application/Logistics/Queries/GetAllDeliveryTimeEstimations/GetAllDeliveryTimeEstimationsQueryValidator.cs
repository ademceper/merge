using FluentValidation;

namespace Merge.Application.Logistics.Queries.GetAllDeliveryTimeEstimations;

public class GetAllDeliveryTimeEstimationsQueryValidator : AbstractValidator<GetAllDeliveryTimeEstimationsQuery>
{
    public GetAllDeliveryTimeEstimationsQueryValidator()
    {
        // No validation needed for optional filters
    }
}

