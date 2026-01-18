using FluentValidation;

namespace Merge.Application.Search.Queries.GetCompleteRecommendations;

public class GetCompleteRecommendationsQueryValidator : AbstractValidator<GetCompleteRecommendationsQuery>
{
    public GetCompleteRecommendationsQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("Kullanıcı ID'si boş olamaz.");
    }
}
