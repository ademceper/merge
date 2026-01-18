using FluentValidation;

namespace Merge.Application.Content.Commands.CreateBlogCategory;

public class CreateBlogCategoryCommandValidator : AbstractValidator<CreateBlogCategoryCommand>
{
    public CreateBlogCategoryCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Kategori adı zorunludur.")
            .MaximumLength(100)
            .WithMessage("Kategori adı en fazla 100 karakter olabilir.")
            .MinimumLength(2)
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
            .WithMessage("Sıralama 0 veya daha büyük olmalıdır.");
    }
}

