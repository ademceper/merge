using FluentValidation;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Product.Commands.DeleteProductTemplate;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class DeleteProductTemplateCommandValidator : AbstractValidator<DeleteProductTemplateCommand>
{
    public DeleteProductTemplateCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Şablon ID boş olamaz.");
    }
}
