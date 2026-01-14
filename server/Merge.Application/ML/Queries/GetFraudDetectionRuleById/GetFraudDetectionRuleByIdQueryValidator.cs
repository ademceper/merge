using FluentValidation;

namespace Merge.Application.ML.Queries.GetFraudDetectionRuleById;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class GetFraudDetectionRuleByIdQueryValidator : AbstractValidator<GetFraudDetectionRuleByIdQuery>
{
    public GetFraudDetectionRuleByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("ID is required.");
    }
}
