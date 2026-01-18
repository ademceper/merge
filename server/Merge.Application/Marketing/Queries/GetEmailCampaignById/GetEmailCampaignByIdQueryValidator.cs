using FluentValidation;

namespace Merge.Application.Marketing.Queries.GetEmailCampaignById;

public class GetEmailCampaignByIdQueryValidator : AbstractValidator<GetEmailCampaignByIdQuery>
{
    public GetEmailCampaignByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Kampanya ID'si zorunludur.");
    }
}
