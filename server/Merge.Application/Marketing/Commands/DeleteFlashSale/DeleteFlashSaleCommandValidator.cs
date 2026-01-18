using FluentValidation;

namespace Merge.Application.Marketing.Commands.DeleteFlashSale;

public class DeleteFlashSaleCommandValidator : AbstractValidator<DeleteFlashSaleCommand>
{
    public DeleteFlashSaleCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Flash Sale ID'si zorunludur.");
    }
}
