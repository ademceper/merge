using FluentValidation;

namespace Merge.Application.Catalog.Commands.UpdateCategory;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class UpdateCategoryCommandValidator : AbstractValidator<UpdateCategoryCommand>
{
    public UpdateCategoryCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Kategori ID'si zorunludur.");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Kategori adi zorunludur.")
            .MaximumLength(200)
            .WithMessage("Kategori adi en fazla 200 karakter olabilir.")
            .MinimumLength(2)
            .WithMessage("Kategori adi en az 2 karakter olmalidir.");

        RuleFor(x => x.Description)
            .MaximumLength(2000)
            .WithMessage("Aciklama en fazla 2000 karakter olabilir.");

        RuleFor(x => x.Slug)
            .NotEmpty()
            .WithMessage("Slug zorunludur.")
            .MaximumLength(200)
            .WithMessage("Slug en fazla 200 karakter olabilir.")
            .MinimumLength(2)
            .WithMessage("Slug en az 2 karakter olmalidir.")
            .Matches(@"^[a-z0-9]+(?:-[a-z0-9]+)*$")
            .WithMessage("Slug sadece kucuk harf, rakam ve tire icerebilir.");

        RuleFor(x => x.ImageUrl)
            .MaximumLength(500)
            .WithMessage("Resim URL'si en fazla 500 karakter olabilir.");
    }
}
