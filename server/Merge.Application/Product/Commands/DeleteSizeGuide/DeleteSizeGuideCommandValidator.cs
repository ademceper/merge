using FluentValidation;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Product.Commands.DeleteSizeGuide;

public class DeleteSizeGuideCommandValidator : AbstractValidator<DeleteSizeGuideCommand>
{
    public DeleteSizeGuideCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Beden kılavuzu ID boş olamaz.");
    }
}
