using FluentValidation;

namespace Merge.Application.LiveCommerce.Queries.GetStreamStats;

// ✅ BOLUM 2.0: FluentValidation (ZORUNLU)
public class GetStreamStatsQueryValidator : AbstractValidator<GetStreamStatsQuery>
{
    public GetStreamStatsQueryValidator()
    {
        RuleFor(x => x.StreamId)
            .NotEmpty().WithMessage("Stream ID'si zorunludur.");
    }
}

