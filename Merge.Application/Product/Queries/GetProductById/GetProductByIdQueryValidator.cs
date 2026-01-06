using FluentValidation;

namespace Merge.Application.Product.Queries.GetProductById;

// BOLUM 2.0: FluentValidation (ZORUNLU)
public class GetProductByIdQueryValidator : AbstractValidator<GetProductByIdQuery>
{
    public GetProductByIdQueryValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty()
            .WithMessage("Urun ID'si zorunludur.");
    }
}
