using FluentValidation;

namespace Merge.Application.Marketing.Queries.GetAllEmailAutomations;

// ✅ BOLUM 2.0: FluentValidation (ZORUNLU)
public class GetAllEmailAutomationsQueryValidator : AbstractValidator<GetAllEmailAutomationsQuery>
{
    public GetAllEmailAutomationsQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0).WithMessage("Sayfa numarası 0'dan büyük olmalıdır.");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("Sayfa boyutu 0'dan büyük olmalıdır.")
            .LessThanOrEqualTo(100).WithMessage("Sayfa boyutu en fazla 100 olabilir.");
    }
}
