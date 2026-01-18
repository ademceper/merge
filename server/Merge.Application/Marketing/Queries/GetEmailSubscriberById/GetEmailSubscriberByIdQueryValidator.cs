using FluentValidation;

namespace Merge.Application.Marketing.Queries.GetEmailSubscriberById;

public class GetEmailSubscriberByIdQueryValidator : AbstractValidator<GetEmailSubscriberByIdQuery>
{
    public GetEmailSubscriberByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Subscriber ID zorunludur.");
    }
}
