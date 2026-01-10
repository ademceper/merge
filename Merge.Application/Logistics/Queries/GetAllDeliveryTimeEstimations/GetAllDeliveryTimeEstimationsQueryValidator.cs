using FluentValidation;

namespace Merge.Application.Logistics.Queries.GetAllDeliveryTimeEstimations;

// ✅ BOLUM 2.0: FluentValidation (ZORUNLU)
public class GetAllDeliveryTimeEstimationsQueryValidator : AbstractValidator<GetAllDeliveryTimeEstimationsQuery>
{
    public GetAllDeliveryTimeEstimationsQueryValidator()
    {
        // No validation needed for optional filters
    }
}

