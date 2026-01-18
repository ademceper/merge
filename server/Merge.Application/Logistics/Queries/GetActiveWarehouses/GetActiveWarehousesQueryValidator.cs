using FluentValidation;

namespace Merge.Application.Logistics.Queries.GetActiveWarehouses;

public class GetActiveWarehousesQueryValidator : AbstractValidator<GetActiveWarehousesQuery>
{
    public GetActiveWarehousesQueryValidator()
    {
        // No validation needed for empty query
    }
}

