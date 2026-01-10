using FluentValidation;

namespace Merge.Application.Logistics.Queries.GetAllWarehouses;

// ✅ BOLUM 2.0: FluentValidation (ZORUNLU)
public class GetAllWarehousesQueryValidator : AbstractValidator<GetAllWarehousesQuery>
{
    public GetAllWarehousesQueryValidator()
    {
        // No validation needed for boolean parameter
    }
}

