using FluentValidation;

namespace Merge.Application.Governance.Queries.GetEntityHistory;

// ✅ BOLUM 2.3: FluentValidation (ZORUNLU)
public class GetEntityHistoryQueryValidator : AbstractValidator<GetEntityHistoryQuery>
{
    public GetEntityHistoryQueryValidator()
    {
        RuleFor(x => x.EntityType)
            .NotEmpty().WithMessage("Entity type gereklidir")
            .MaximumLength(100).WithMessage("Entity type en fazla 100 karakter olabilir");

        RuleFor(x => x.EntityId)
            .NotEmpty().WithMessage("Entity ID gereklidir");
    }
}

