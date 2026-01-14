using FluentValidation;
using Microsoft.Extensions.Options;
using Merge.Application.Configuration;

namespace Merge.Application.User.Queries.SearchActivities;

// ✅ BOLUM 2.0: FluentValidation (ZORUNLU)
// ✅ BOLUM 12.0: Configuration - Magic number'lar configuration'dan alınıyor
public class SearchActivitiesQueryValidator : AbstractValidator<SearchActivitiesQuery>
{
    public SearchActivitiesQueryValidator(IOptions<PaginationSettings> paginationSettings)
    {
        var paginationConfig = paginationSettings.Value;

        RuleFor(x => x.Filter)
            .NotNull()
            .WithMessage("Filtre bilgisi zorunludur.");

        When(x => x.Filter != null, () =>
        {
            RuleFor(x => x.Filter!.PageNumber)
                .GreaterThanOrEqualTo(1)
                .WithMessage("Sayfa numarası en az 1 olmalıdır.");

            RuleFor(x => x.Filter!.PageSize)
                .GreaterThan(0)
                .WithMessage("Sayfa boyutu 0'dan büyük olmalıdır.")
                .LessThanOrEqualTo(paginationConfig.MaxPageSize)
                .WithMessage($"Sayfa boyutu en fazla {paginationConfig.MaxPageSize} olabilir.");

            RuleFor(x => x.Filter!.StartDate)
                .LessThanOrEqualTo(x => x.Filter!.EndDate)
                .When(x => x.Filter!.StartDate.HasValue && x.Filter!.EndDate.HasValue)
                .WithMessage("Başlangıç tarihi bitiş tarihinden sonra olamaz.");
        });
    }
}
