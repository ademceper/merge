using FluentValidation;

namespace Merge.Application.Content.Queries.GetAllPageBuilders;

// ✅ BOLUM 2.3: FluentValidation (ZORUNLU)
public class GetAllPageBuildersQueryValidator : AbstractValidator<GetAllPageBuildersQuery>
{
    public GetAllPageBuildersQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0).WithMessage("Sayfa numarası 0'dan büyük olmalıdır");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("Sayfa boyutu 0'dan büyük olmalıdır")
            .LessThanOrEqualTo(100).WithMessage("Sayfa boyutu en fazla 100 olabilir");
    }
}

