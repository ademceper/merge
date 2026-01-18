using FluentValidation;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Product.Commands.DeleteProductBundle;

public class DeleteProductBundleCommandValidator : AbstractValidator<DeleteProductBundleCommand>
{
    public DeleteProductBundleCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Paket ID boş olamaz.");
    }
}
