using FluentValidation;

namespace Merge.Application.Logistics.Queries.GetAvailableShippingProviders;

// ✅ BOLUM 2.0: FluentValidation (ZORUNLU)
public class GetAvailableShippingProvidersQueryValidator : AbstractValidator<GetAvailableShippingProvidersQuery>
{
    public GetAvailableShippingProvidersQueryValidator()
    {
        // No validation needed for empty query
    }
}

