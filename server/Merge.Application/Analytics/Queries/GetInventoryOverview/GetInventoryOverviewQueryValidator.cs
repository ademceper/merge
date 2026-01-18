using FluentValidation;

namespace Merge.Application.Analytics.Queries.GetInventoryOverview;

public class GetInventoryOverviewQueryValidator : AbstractValidator<GetInventoryOverviewQuery>
{
    public GetInventoryOverviewQueryValidator()
    {
        // GetInventoryOverviewQuery parametre almadığı için validation gerekmez
        // Ancak FluentValidation pattern'i için boş validator oluşturuyoruz
    }
}

