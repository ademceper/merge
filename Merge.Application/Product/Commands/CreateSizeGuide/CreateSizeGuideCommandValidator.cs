using FluentValidation;
using Merge.Application.DTOs.Product;

namespace Merge.Application.Product.Commands.CreateSizeGuide;

// ✅ BOLUM 2.0: FluentValidation (ZORUNLU)
public class CreateSizeGuideCommandValidator : AbstractValidator<CreateSizeGuideCommand>
{
    public CreateSizeGuideCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Beden kılavuzu adı boş olamaz.")
            .MinimumLength(2).WithMessage("Beden kılavuzu adı en az 2 karakter olmalıdır.")
            .MaximumLength(200).WithMessage("Beden kılavuzu adı en fazla 200 karakter olmalıdır.");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Açıklama en fazla 2000 karakter olabilir.");

        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("Kategori ID boş olamaz.");

        RuleFor(x => x.Brand)
            .MaximumLength(100).WithMessage("Marka en fazla 100 karakter olabilir.");

        RuleFor(x => x.Type)
            .NotEmpty().WithMessage("Tip boş olamaz.");

        RuleFor(x => x.MeasurementUnit)
            .NotEmpty().WithMessage("Ölçü birimi boş olamaz.")
            .MaximumLength(20).WithMessage("Ölçü birimi en fazla 20 karakter olabilir.");

        RuleFor(x => x.Entries)
            .NotEmpty().WithMessage("En az bir beden girişi gereklidir.");

        RuleForEach(x => x.Entries).SetValidator(new CreateSizeGuideEntryDtoValidator());
    }
}

public class CreateSizeGuideEntryDtoValidator : AbstractValidator<CreateSizeGuideEntryDto>
{
    public CreateSizeGuideEntryDtoValidator()
    {
        RuleFor(x => x.SizeLabel)
            .NotEmpty().WithMessage("Beden etiketi boş olamaz.");

        RuleFor(x => x.DisplayOrder)
            .GreaterThanOrEqualTo(0).WithMessage("Görüntüleme sırası 0 veya daha büyük olmalıdır.");
    }
}
