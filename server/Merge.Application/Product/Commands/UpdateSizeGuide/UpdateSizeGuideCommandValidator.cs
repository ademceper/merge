using FluentValidation;
using Merge.Domain.Enums;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Product.Commands.UpdateSizeGuide;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class UpdateSizeGuideCommandValidator : AbstractValidator<UpdateSizeGuideCommand>
{
    public UpdateSizeGuideCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Beden kılavuzu ID boş olamaz.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Beden kılavuzu adı boş olamaz.")
            .MinimumLength(2).WithMessage("Beden kılavuzu adı en az 2 karakter olmalıdır.")
            .MaximumLength(200).WithMessage("Beden kılavuzu adı en fazla 200 karakter olmalıdır.");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Açıklama en fazla 2000 karakter olabilir.");

        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("Kategori ID boş olamaz.");

        RuleFor(x => x.Type)
            .NotEmpty().WithMessage("Tip boş olamaz.")
            .Must(BeValidSizeGuideType).WithMessage("Geçerli bir beden kılavuzu tipi seçiniz.");

        RuleFor(x => x.MeasurementUnit)
            .NotEmpty().WithMessage("Ölçü birimi boş olamaz.")
            .MaximumLength(20).WithMessage("Ölçü birimi en fazla 20 karakter olabilir.");

        RuleFor(x => x.Brand)
            .MaximumLength(100).WithMessage("Marka en fazla 100 karakter olabilir.");
    }

    private bool BeValidSizeGuideType(string type)
    {
        return System.Enum.TryParse<Merge.Domain.Enums.SizeGuideType>(type, true, out _);
    }
}
