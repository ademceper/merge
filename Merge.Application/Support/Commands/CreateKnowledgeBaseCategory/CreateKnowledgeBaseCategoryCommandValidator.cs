using FluentValidation;
using Merge.Application.Configuration;
using Microsoft.Extensions.Options;

namespace Merge.Application.Support.Commands.CreateKnowledgeBaseCategory;

// ✅ BOLUM 2.1: Pipeline Behaviors - ValidationBehavior (ZORUNLU)
public class CreateKnowledgeBaseCategoryCommandValidator : AbstractValidator<CreateKnowledgeBaseCategoryCommand>
{
    public CreateKnowledgeBaseCategoryCommandValidator(IOptions<SupportSettings> settings)
    {
        var supportSettings = settings.Value;

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Kategori adı boş olamaz")
            .MinimumLength(supportSettings.MinCategoryNameLength).WithMessage($"Kategori adı en az {supportSettings.MinCategoryNameLength} karakter olmalıdır")
            .MaximumLength(supportSettings.MaxCategoryNameLength)
            .WithMessage($"Kategori adı en fazla {supportSettings.MaxCategoryNameLength} karakter olmalıdır");

        When(x => !string.IsNullOrEmpty(x.Description), () =>
        {
            RuleFor(x => x.Description)
                .MaximumLength(supportSettings.MaxCategoryDescriptionLength)
                .WithMessage($"Açıklama en fazla {supportSettings.MaxCategoryDescriptionLength} karakter olmalıdır");
        });

        When(x => !string.IsNullOrEmpty(x.IconUrl), () =>
        {
            RuleFor(x => x.IconUrl)
                .Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out _))
                .WithMessage("Geçerli bir URL giriniz")
                .MaximumLength(supportSettings.MaxIconUrlLength).WithMessage($"Icon URL en fazla {supportSettings.MaxIconUrlLength} karakter olmalıdır");
        });

        RuleFor(x => x.DisplayOrder)
            .GreaterThanOrEqualTo(supportSettings.MinDisplayOrder).WithMessage($"Görüntüleme sırası {supportSettings.MinDisplayOrder} veya daha büyük olmalıdır");
    }
}
