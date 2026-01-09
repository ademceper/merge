using FluentValidation;

namespace Merge.Application.Product.Commands.DeleteProduct;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class DeleteProductCommandValidator : AbstractValidator<DeleteProductCommand>
{
    public DeleteProductCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Ürün ID'si zorunludur.");
    }
}

