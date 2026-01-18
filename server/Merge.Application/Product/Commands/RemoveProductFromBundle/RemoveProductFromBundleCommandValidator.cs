using FluentValidation;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Product.Commands.RemoveProductFromBundle;

public class RemoveProductFromBundleCommandValidator : AbstractValidator<RemoveProductFromBundleCommand>
{
    public RemoveProductFromBundleCommandValidator()
    {
        RuleFor(x => x.BundleId)
            .NotEmpty().WithMessage("Paket ID boş olamaz.");

        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Ürün ID boş olamaz.");
    }
}
