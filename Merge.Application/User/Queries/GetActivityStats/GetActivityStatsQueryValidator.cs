using FluentValidation;
using Microsoft.Extensions.Options;
using Merge.Application.Configuration;

namespace Merge.Application.User.Queries.GetActivityStats;

// ✅ BOLUM 2.0: FluentValidation (ZORUNLU)
// ✅ BOLUM 12.0: Configuration - Magic number'lar configuration'dan alınıyor
public class GetActivityStatsQueryValidator : AbstractValidator<GetActivityStatsQuery>
{
    public GetActivityStatsQueryValidator(IOptions<UserSettings> userSettings)
    {
        var settings = userSettings.Value;

        RuleFor(x => x.Days)
            .GreaterThan(0)
            .WithMessage("Gün sayısı 0'dan büyük olmalıdır.")
            .LessThanOrEqualTo(settings.Activity.MaxDays)
            .WithMessage($"Gün sayısı en fazla {settings.Activity.MaxDays} olabilir.");
    }
}
