using FluentValidation;
using Microsoft.Extensions.Options;
using Merge.Application.Configuration;

namespace Merge.Application.User.Queries.GetUserSessions;

public class GetUserSessionsQueryValidator : AbstractValidator<GetUserSessionsQuery>
{
    public GetUserSessionsQueryValidator(IOptions<UserSettings> userSettings)
    {
        var settings = userSettings.Value;

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("Kullanıcı ID'si zorunludur.");

        RuleFor(x => x.Days)
            .GreaterThan(0)
            .WithMessage("Gün sayısı 0'dan büyük olmalıdır.")
            .LessThanOrEqualTo(settings.Activity.MaxSessionDays)
            .WithMessage($"Gün sayısı en fazla {settings.Activity.MaxSessionDays} olabilir.");
    }
}
