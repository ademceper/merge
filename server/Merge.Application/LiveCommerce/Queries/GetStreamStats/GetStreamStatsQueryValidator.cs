using FluentValidation;

namespace Merge.Application.LiveCommerce.Queries.GetStreamStats;

public class GetStreamStatsQueryValidator : AbstractValidator<GetStreamStatsQuery>
{
    public GetStreamStatsQueryValidator()
    {
        RuleFor(x => x.StreamId)
            .NotEmpty().WithMessage("Stream ID'si zorunludur.");
    }
}
