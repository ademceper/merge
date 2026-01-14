using FluentValidation;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Product.Commands.AddProductToComparison;

// ✅ BOLUM 2.0: FluentValidation (ZORUNLU)
public class AddProductToComparisonCommandValidator : AbstractValidator<AddProductToComparisonCommand>
{
    public AddProductToComparisonCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Kullanıcı ID boş olamaz.");

        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Ürün ID boş olamaz.");
    }
}
