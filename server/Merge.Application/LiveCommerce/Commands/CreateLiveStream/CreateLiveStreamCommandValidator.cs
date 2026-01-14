using FluentValidation;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.LiveCommerce.Commands.CreateLiveStream;

// ✅ BOLUM 2.0: FluentValidation (ZORUNLU)
public class CreateLiveStreamCommandValidator : AbstractValidator<CreateLiveStreamCommand>
{
    public CreateLiveStreamCommandValidator()
    {
        RuleFor(x => x.SellerId)
            .NotEmpty().WithMessage("Satıcı ID'si zorunludur.");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Başlık zorunludur.")
            .MaximumLength(200).WithMessage("Başlık en fazla 200 karakter olabilir.")
            .MinimumLength(2).WithMessage("Başlık en az 2 karakter olmalıdır.");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Açıklama en fazla 2000 karakter olabilir.");

        RuleFor(x => x.StreamUrl)
            .Must(uri => string.IsNullOrEmpty(uri) || Uri.TryCreate(uri, UriKind.Absolute, out _))
            .WithMessage("Geçerli bir URL giriniz.")
            .MaximumLength(500).WithMessage("Stream URL en fazla 500 karakter olabilir.");

        RuleFor(x => x.StreamKey)
            .MaximumLength(200).WithMessage("Stream key en fazla 200 karakter olabilir.");

        RuleFor(x => x.ThumbnailUrl)
            .Must(uri => string.IsNullOrEmpty(uri) || Uri.TryCreate(uri, UriKind.Absolute, out _))
            .WithMessage("Geçerli bir URL giriniz.")
            .MaximumLength(500).WithMessage("Thumbnail URL en fazla 500 karakter olabilir.");

        RuleFor(x => x.Category)
            .MaximumLength(100).WithMessage("Kategori en fazla 100 karakter olabilir.");

        RuleFor(x => x.Tags)
            .MaximumLength(500).WithMessage("Etiketler en fazla 500 karakter olabilir.");
    }
}

