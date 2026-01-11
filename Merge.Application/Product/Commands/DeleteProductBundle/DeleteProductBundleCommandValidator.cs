using FluentValidation;

namespace Merge.Application.Product.Commands.DeleteProductBundle;

// ✅ BOLUM 2.0: FluentValidation (ZORUNLU)
public class DeleteProductBundleCommandValidator : AbstractValidator<DeleteProductBundleCommand>
{
    public DeleteProductBundleCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Paket ID boş olamaz.");
    }
}
