using FluentValidation;

namespace Merge.Application.B2B.Commands.DeleteVolumeDiscount;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class DeleteVolumeDiscountCommandValidator : AbstractValidator<DeleteVolumeDiscountCommand>
{
    public DeleteVolumeDiscountCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("İndirim ID boş olamaz");
    }
}

