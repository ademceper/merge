using FluentValidation;

namespace Merge.Application.Analytics.Commands.GenerateReport;

// ✅ BOLUM 2.1: Pipeline Behaviors - ValidationBehavior (ZORUNLU)
public class GenerateReportCommandValidator : AbstractValidator<GenerateReportCommand>
{
    public GenerateReportCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Kullanıcı ID boş olamaz");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Rapor adı zorunludur")
            .MaximumLength(200).WithMessage("Rapor adı en fazla 200 karakter olabilir")
            .MinimumLength(2).WithMessage("Rapor adı en az 2 karakter olmalıdır");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Açıklama en fazla 1000 karakter olabilir");

        RuleFor(x => x.Type)
            .NotEmpty().WithMessage("Rapor tipi zorunludur")
            .Must(type => Enum.TryParse<Merge.Domain.Enums.ReportType>(type, true, out _))
            .WithMessage("Geçersiz rapor tipi");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Başlangıç tarihi zorunludur")
            .LessThan(x => x.EndDate).WithMessage("Başlangıç tarihi bitiş tarihinden önce olmalıdır");

        RuleFor(x => x.EndDate)
            .NotEmpty().WithMessage("Bitiş tarihi zorunludur")
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Bitiş tarihi gelecekte olamaz");

        RuleFor(x => x.Format)
            .MaximumLength(50).WithMessage("Format en fazla 50 karakter olabilir")
            .Must(format => string.IsNullOrEmpty(format) || Enum.TryParse<Merge.Domain.Enums.ReportFormat>(format, true, out _))
            .WithMessage("Geçersiz format");
    }
}

