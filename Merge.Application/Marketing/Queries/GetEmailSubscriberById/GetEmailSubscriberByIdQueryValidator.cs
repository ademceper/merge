using FluentValidation;

namespace Merge.Application.Marketing.Queries.GetEmailSubscriberById;

// ✅ BOLUM 2.0: FluentValidation (ZORUNLU)
public class GetEmailSubscriberByIdQueryValidator : AbstractValidator<GetEmailSubscriberByIdQuery>
{
    public GetEmailSubscriberByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Subscriber ID zorunludur.");
    }
}
