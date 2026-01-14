using FluentValidation;

namespace Merge.Application.Search.Queries.GetCompleteRecommendations;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class GetCompleteRecommendationsQueryValidator : AbstractValidator<GetCompleteRecommendationsQuery>
{
    public GetCompleteRecommendationsQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("Kullanıcı ID'si boş olamaz.");
    }
}
