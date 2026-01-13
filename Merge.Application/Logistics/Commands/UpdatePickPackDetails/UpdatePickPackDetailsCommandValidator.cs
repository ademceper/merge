using FluentValidation;
using Merge.Domain.Enums;

namespace Merge.Application.Logistics.Commands.UpdatePickPackDetails;

// ✅ BOLUM 2.0: FluentValidation (ZORUNLU)
public class UpdatePickPackDetailsCommandValidator : AbstractValidator<UpdatePickPackDetailsCommand>
{
    public UpdatePickPackDetailsCommandValidator()
    {
        RuleFor(x => x.PickPackId)
            .NotEmpty().WithMessage("Pick-pack ID'si zorunludur.");

        RuleFor(x => x.Notes)
            .MaximumLength(2000).WithMessage("Notlar en fazla 2000 karakter olabilir.")
            .When(x => !string.IsNullOrEmpty(x.Notes));

        RuleFor(x => x.Weight)
            .GreaterThanOrEqualTo(0).WithMessage("Ağırlık 0 veya daha büyük olmalıdır.")
            .When(x => x.Weight.HasValue);

        RuleFor(x => x.Dimensions)
            .MaximumLength(100).WithMessage("Boyutlar en fazla 100 karakter olabilir.")
            .When(x => !string.IsNullOrEmpty(x.Dimensions));

        RuleFor(x => x.PackageCount)
            .GreaterThan(0).WithMessage("Paket sayısı 0'dan büyük olmalıdır.")
            .When(x => x.PackageCount.HasValue);
    }
}

