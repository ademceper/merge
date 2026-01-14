using FluentValidation;

namespace Merge.Application.B2B.Commands.DeleteWholesalePrice;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class DeleteWholesalePriceCommandValidator : AbstractValidator<DeleteWholesalePriceCommand>
{
    public DeleteWholesalePriceCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Fiyat ID boş olamaz");
    }
}

