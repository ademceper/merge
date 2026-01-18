using FluentValidation;
using Merge.Domain.Enums;

namespace Merge.Application.Analytics.Commands.CreateReportSchedule;

public class CreateReportScheduleCommandValidator : AbstractValidator<CreateReportScheduleCommand>
{
    public CreateReportScheduleCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Kullanıcı ID boş olamaz");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Zamanlama adı zorunludur")
            .MaximumLength(200).WithMessage("Zamanlama adı en fazla 200 karakter olabilir")
            .MinimumLength(2).WithMessage("Zamanlama adı en az 2 karakter olmalıdır");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Açıklama en fazla 1000 karakter olabilir");

        RuleFor(x => x.Type)
            .NotEmpty().WithMessage("Rapor tipi zorunludur")
            .Must(type => Enum.TryParse<Merge.Domain.Enums.ReportType>(type, true, out _))
            .WithMessage("Geçersiz rapor tipi");

        RuleFor(x => x.Frequency)
            .NotEmpty().WithMessage("Sıklık zorunludur")
            .Must(frequency => Enum.TryParse<Merge.Domain.Enums.ReportFrequency>(frequency, true, out _))
            .WithMessage("Geçersiz sıklık");

        RuleFor(x => x.DayOfWeek)
            .InclusiveBetween(1, 7).WithMessage("Haftanın günü 1 (Pazartesi) ile 7 (Pazar) arasında olmalıdır");

        RuleFor(x => x.DayOfMonth)
            .InclusiveBetween(1, 31).WithMessage("Ayın günü 1 ile 31 arasında olmalıdır");

        RuleFor(x => x.Format)
            .MaximumLength(50).WithMessage("Format en fazla 50 karakter olabilir")
            .Must(format => string.IsNullOrEmpty(format) || Enum.TryParse<Merge.Domain.Enums.ReportFormat>(format, true, out _))
            .WithMessage("Geçersiz format");

        RuleFor(x => x.EmailRecipients)
            .MaximumLength(1000).WithMessage("E-posta alıcıları en fazla 1000 karakter olabilir");
    }
}

