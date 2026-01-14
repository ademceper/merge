using FluentValidation;

namespace Merge.Application.LiveCommerce.Queries.GetLiveStream;

// ✅ BOLUM 2.0: FluentValidation (ZORUNLU)
public class GetLiveStreamQueryValidator : AbstractValidator<GetLiveStreamQuery>
{
    public GetLiveStreamQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Stream ID'si zorunludur.");
    }
}

