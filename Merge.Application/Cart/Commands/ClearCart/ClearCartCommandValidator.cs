using FluentValidation;

namespace Merge.Application.Cart.Commands.ClearCart;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class ClearCartCommandValidator : AbstractValidator<ClearCartCommand>
{
    public ClearCartCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Kullanıcı ID zorunludur");
    }
}

