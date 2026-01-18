using FluentValidation;

namespace Merge.Application.Logistics.Commands.CompletePacking;

public class CompletePackingCommandValidator : AbstractValidator<CompletePackingCommand>
{
    public CompletePackingCommandValidator()
    {
        RuleFor(x => x.PickPackId)
            .NotEmpty().WithMessage("Pick-pack ID'si zorunludur.");

        RuleFor(x => x.Weight)
            .GreaterThan(0).WithMessage("Ağırlık 0'dan büyük olmalıdır.");

        RuleFor(x => x.PackageCount)
            .GreaterThan(0).WithMessage("Paket sayısı 0'dan büyük olmalıdır.");

        RuleFor(x => x.Dimensions)
            .MaximumLength(100).WithMessage("Boyutlar en fazla 100 karakter olabilir.")
            .When(x => !string.IsNullOrEmpty(x.Dimensions));
    }
}

