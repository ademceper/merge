using FluentValidation;

namespace Merge.Application.Logistics.Commands.CreatePickPack;

public class CreatePickPackCommandValidator : AbstractValidator<CreatePickPackCommand>
{
    public CreatePickPackCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("Sipariş ID'si zorunludur.");

        RuleFor(x => x.WarehouseId)
            .NotEmpty().WithMessage("Depo ID'si zorunludur.");

        RuleFor(x => x.Notes)
            .MaximumLength(2000).WithMessage("Notlar en fazla 2000 karakter olabilir.")
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }
}

