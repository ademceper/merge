using FluentValidation;
using Merge.Domain.Modules.Identity;

namespace Merge.Application.Governance.Queries.GetUserAuditHistory;

// ✅ BOLUM 2.3: FluentValidation (ZORUNLU)
public class GetUserAuditHistoryQueryValidator : AbstractValidator<GetUserAuditHistoryQuery>
{
    public GetUserAuditHistoryQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID gereklidir");

        RuleFor(x => x.Days)
            .GreaterThan(0).WithMessage("Days 0'dan büyük olmalıdır")
            .LessThanOrEqualTo(365).WithMessage("Days en fazla 365 olabilir");
    }
}

