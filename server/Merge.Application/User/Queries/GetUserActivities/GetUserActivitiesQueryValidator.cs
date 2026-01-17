using FluentValidation;
using Microsoft.Extensions.Options;
using Merge.Application.Configuration;

namespace Merge.Application.User.Queries.GetUserActivities;

public class GetUserActivitiesQueryValidator(IOptions<UserSettings> userSettings) : AbstractValidator<GetUserActivitiesQuery>
{
    private readonly UserSettings settings = userSettings.Value;

    public GetUserActivitiesQueryValidator() : this(Options.Create(new UserSettings()))
    {

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
