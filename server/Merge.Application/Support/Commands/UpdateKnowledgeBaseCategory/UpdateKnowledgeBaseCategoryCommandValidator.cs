using FluentValidation;
using Merge.Application.Configuration;
using Microsoft.Extensions.Options;

namespace Merge.Application.Support.Commands.UpdateKnowledgeBaseCategory;

public class UpdateKnowledgeBaseCategoryCommandValidator(IOptions<SupportSettings> settings) : AbstractValidator<UpdateKnowledgeBaseCategoryCommand>
{
    private readonly SupportSettings config = settings.Value;

    public UpdateKnowledgeBaseCategoryCommandValidator() : this(Options.Create(new SupportSettings()))
    {
        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("Kategori ID boş olamaz");

        When(x => !string.IsNullOrEmpty(x.Name), () =>
        {
            RuleFor(x => x.Name)
                .MinimumLength(config.MinCategoryNameLength).WithMessage($"Kategori adı en az {config.MinCategoryNameLength} karakter olmalıdır")
                .MaximumLength(config.MaxCategoryNameLength)
                .WithMessage($"Kategori adı en fazla {config.MaxCategoryNameLength} karakter olmalıdır");
        });

        When(x => !string.IsNullOrEmpty(x.Description), () =>
        {
            RuleFor(x => x.Description)
                .MaximumLength(config.MaxCategoryDescriptionLength)
                .WithMessage($"Açıklama en fazla {config.MaxCategoryDescriptionLength} karakter olmalıdır");
        });

        When(x => !string.IsNullOrEmpty(x.IconUrl), () =>
        {
            RuleFor(x => x.IconUrl)
                .Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out _))
                .WithMessage("Geçerli bir URL giriniz")
                .MaximumLength(config.MaxIconUrlLength).WithMessage($"Icon URL en fazla {config.MaxIconUrlLength} karakter olmalıdır");
        });

        When(x => x.DisplayOrder.HasValue, () =>
        {
            RuleFor(x => x.DisplayOrder)
                .GreaterThanOrEqualTo(config.MinDisplayOrder).WithMessage($"Görüntüleme sırası {config.MinDisplayOrder} veya daha büyük olmalıdır");
        });
    }
}
