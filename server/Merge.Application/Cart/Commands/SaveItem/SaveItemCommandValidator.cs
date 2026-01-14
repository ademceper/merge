using FluentValidation;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.Cart.Commands.SaveItem;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class SaveItemCommandValidator : AbstractValidator<SaveItemCommand>
{
    public SaveItemCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Kullanıcı ID zorunludur");

        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Ürün ID zorunludur");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Miktar 0'dan büyük olmalıdır");

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notlar en fazla 1000 karakter olabilir");
    }
}

