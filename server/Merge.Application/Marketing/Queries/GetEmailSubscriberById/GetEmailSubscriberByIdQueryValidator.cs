using FluentValidation;

namespace Merge.Application.Marketing.Queries.GetEmailSubscriberById;

// ✅ BOLUM 2.0: FluentValidation (ZORUNLU)
// ✅ BOLUM 7.1.8: Primary Constructors (C# 12) - Modern .NET 9 feature
public class GetEmailSubscriberByIdQueryValidator : AbstractValidator<GetEmailSubscriberByIdQuery>
{
    public GetEmailSubscriberByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Subscriber ID zorunludur.");
    }
}
