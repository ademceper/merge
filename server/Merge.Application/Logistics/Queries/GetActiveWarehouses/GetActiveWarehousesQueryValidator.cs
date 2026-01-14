using FluentValidation;

namespace Merge.Application.Logistics.Queries.GetActiveWarehouses;

// ✅ BOLUM 2.0: FluentValidation (ZORUNLU)
public class GetActiveWarehousesQueryValidator : AbstractValidator<GetActiveWarehousesQuery>
{
    public GetActiveWarehousesQueryValidator()
    {
        // No validation needed for empty query
    }
}

