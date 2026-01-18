using FluentValidation;

namespace Merge.Application.Logistics.Queries.GetAllWarehouses;

public class GetAllWarehousesQueryValidator : AbstractValidator<GetAllWarehousesQuery>
{
    public GetAllWarehousesQueryValidator()
    {
        // No validation needed for boolean parameter
    }
}

