using FluentValidation;
using Microsoft.Extensions.Options;
using Merge.Application.Configuration;

namespace Merge.Application.Search.Queries.GetBasedOnViewHistory;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class GetBasedOnViewHistoryQueryValidator(IOptions<SearchSettings> searchSettings) : AbstractValidator<GetBasedOnViewHistoryQuery>
{
    private readonly SearchSettings config = searchSettings.Value;

    public GetBasedOnViewHistoryQueryValidator() : this(Options.Create(new SearchSettings()))
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("Kullanıcı ID'si boş olamaz.");

        RuleFor(x => x.MaxResults)
            .GreaterThan(0)
            .WithMessage("Maksimum sonuç sayısı 1'den büyük olmalıdır.")
            .LessThanOrEqualTo(config.MaxRecommendationResults)
            .WithMessage($"Maksimum sonuç sayısı en fazla {config.MaxRecommendationResults} olabilir.");
    }
}
