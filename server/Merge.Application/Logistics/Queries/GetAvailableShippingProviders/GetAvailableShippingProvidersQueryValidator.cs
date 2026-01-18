using FluentValidation;

namespace Merge.Application.Logistics.Queries.GetAvailableShippingProviders;

public class GetAvailableShippingProvidersQueryValidator : AbstractValidator<GetAvailableShippingProvidersQuery>
{
    public GetAvailableShippingProvidersQueryValidator()
    {
        // No validation needed for empty query
    }
}

