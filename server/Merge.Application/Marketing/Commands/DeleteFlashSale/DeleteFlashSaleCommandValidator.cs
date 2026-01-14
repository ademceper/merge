using FluentValidation;

namespace Merge.Application.Marketing.Commands.DeleteFlashSale;

// ✅ BOLUM 2.0: FluentValidation (ZORUNLU)
// ✅ BOLUM 7.1.8: Primary Constructors (C# 12) - Modern .NET 9 feature
public class DeleteFlashSaleCommandValidator : AbstractValidator<DeleteFlashSaleCommand>
{
    public DeleteFlashSaleCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Flash Sale ID'si zorunludur.");
    }
}
