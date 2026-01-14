using FluentValidation;

namespace Merge.Application.Marketing.Queries.GetEmailCampaignById;

// ✅ BOLUM 7.1.8: Primary Constructors (C# 12) - Modern .NET 9 feature
public class GetEmailCampaignByIdQueryValidator : AbstractValidator<GetEmailCampaignByIdQuery>
{
    public GetEmailCampaignByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Kampanya ID'si zorunludur.");
    }
}
