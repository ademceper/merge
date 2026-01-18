using FluentValidation;

namespace Merge.Application.ML.Queries.GetFraudDetectionRuleById;

public class GetFraudDetectionRuleByIdQueryValidator : AbstractValidator<GetFraudDetectionRuleByIdQuery>
{
    public GetFraudDetectionRuleByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("ID is required.");
    }
}
