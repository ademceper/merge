using FluentValidation;

namespace Merge.Application.Content.Commands.UpdateBlogCategory;

public class UpdateBlogCategoryCommandValidator : AbstractValidator<UpdateBlogCategoryCommand>
{
    public UpdateBlogCategoryCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Kategori ID'si zorunludur.");

        RuleFor(x => x.Name)
            .MaximumLength(100)
            .When(x => !string.IsNullOrEmpty(x.Name))
            .WithMessage("Kategori adı en fazla 100 karakter olabilir.")
            .MinimumLength(2)
            .When(x => !string.IsNullOrEmpty(x.Name))
            .WithMessage("Kategori adı en az 2 karakter olmalıdır.");

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.Description))
            .WithMessage("Açıklama en fazla 500 karakter olabilir.");

        RuleFor(x => x.ImageUrl)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.ImageUrl))
            .WithMessage("Görsel URL'i en fazla 500 karakter olabilir.");

        RuleFor(x => x.DisplayOrder)
            .GreaterThanOrEqualTo(0)
            .When(x => x.DisplayOrder.HasValue)
            .WithMessage("Sıralama 0 veya daha büyük olmalıdır.");
    }
}

