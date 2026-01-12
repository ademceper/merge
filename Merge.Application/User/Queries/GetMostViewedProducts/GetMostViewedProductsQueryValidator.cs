using FluentValidation;
using Microsoft.Extensions.Options;
using Merge.Application.Configuration;

namespace Merge.Application.User.Queries.GetMostViewedProducts;

// ✅ BOLUM 2.0: FluentValidation (ZORUNLU)
// ✅ BOLUM 12.0: Configuration - Magic number'lar configuration'dan alınıyor
public class GetMostViewedProductsQueryValidator : AbstractValidator<GetMostViewedProductsQuery>
{
    public GetMostViewedProductsQueryValidator(IOptions<UserSettings> userSettings)
    {
        var settings = userSettings.Value;

        RuleFor(x => x.Days)
            .GreaterThan(0)
            .WithMessage("Gün sayısı 0'dan büyük olmalıdır.")
            .LessThanOrEqualTo(settings.Activity.MaxDays)
            .WithMessage($"Gün sayısı en fazla {settings.Activity.MaxDays} olabilir.");

        RuleFor(x => x.TopN)
            .GreaterThan(0)
            .WithMessage("Top N değeri 0'dan büyük olmalıdır.")
            .LessThanOrEqualTo(settings.Activity.MaxTopN)
            .WithMessage($"Top N değeri en fazla {settings.Activity.MaxTopN} olabilir.");
    }
}
