using FluentValidation;
using Microsoft.Extensions.Options;
using Merge.Application.Configuration;

namespace Merge.Application.User.Queries.GetUserActivities;

// ✅ BOLUM 2.0: FluentValidation (ZORUNLU)
// ✅ BOLUM 12.0: Configuration - Magic number'lar configuration'dan alınıyor
public class GetUserActivitiesQueryValidator : AbstractValidator<GetUserActivitiesQuery>
{
    public GetUserActivitiesQueryValidator(IOptions<UserSettings> userSettings)
    {
        var settings = userSettings.Value;

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("Kullanıcı ID'si zorunludur.");

        RuleFor(x => x.Days)
            .GreaterThan(0)
            .WithMessage("Gün sayısı 0'dan büyük olmalıdır.")
            .LessThanOrEqualTo(settings.Activity.MaxDays)
            .WithMessage($"Gün sayısı en fazla {settings.Activity.MaxDays} olabilir.");
    }
}
