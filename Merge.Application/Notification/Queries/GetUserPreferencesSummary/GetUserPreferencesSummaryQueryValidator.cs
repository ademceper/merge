using FluentValidation;

namespace Merge.Application.Notification.Queries.GetUserPreferencesSummary;

/// <summary>
/// Get User Preferences Summary Query Validator - BOLUM 2.1: FluentValidation (ZORUNLU)
/// </summary>
public class GetUserPreferencesSummaryQueryValidator : AbstractValidator<GetUserPreferencesSummaryQuery>
{
    public GetUserPreferencesSummaryQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("Kullanıcı ID'si zorunludur.");
    }
}
