using FluentValidation;

namespace Merge.Application.Product.Queries.GetProductBundleById;

// ✅ BOLUM 2.0: FluentValidation (ZORUNLU)
public class GetProductBundleByIdQueryValidator : AbstractValidator<GetProductBundleByIdQuery>
{
    public GetProductBundleByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Paket ID boş olamaz.");
    }
}
