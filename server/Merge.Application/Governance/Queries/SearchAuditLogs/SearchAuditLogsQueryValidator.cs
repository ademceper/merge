using FluentValidation;
using Microsoft.Extensions.Options;
using Merge.Application.Configuration;
using Merge.Domain.SharedKernel;

namespace Merge.Application.Governance.Queries.SearchAuditLogs;

public class SearchAuditLogsQueryValidator(IOptions<PaginationSettings> paginationSettings) : AbstractValidator<SearchAuditLogsQuery>
{
    private readonly PaginationSettings config = paginationSettings.Value;

    public SearchAuditLogsQueryValidator() : this(Options.Create(new PaginationSettings()))
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0).WithMessage("Page number 0'dan büyük olmalıdır");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("Page size 0'dan büyük olmalıdır")
            .LessThanOrEqualTo(config.MaxPageSize).WithMessage($"Page size en fazla {config.MaxPageSize} olabilir");

        RuleFor(x => x.UserEmail)
            .EmailAddress().WithMessage("Geçerli bir e-posta adresi giriniz")
            .When(x => !string.IsNullOrEmpty(x.UserEmail));

        RuleFor(x => x.Action)
            .MaximumLength(100).WithMessage("Action en fazla 100 karakter olabilir")
            .When(x => !string.IsNullOrEmpty(x.Action));

        RuleFor(x => x.EntityType)
            .MaximumLength(100).WithMessage("Entity type en fazla 100 karakter olabilir")
            .When(x => !string.IsNullOrEmpty(x.EntityType));

        RuleFor(x => x.TableName)
            .MaximumLength(100).WithMessage("Table name en fazla 100 karakter olabilir")
            .When(x => !string.IsNullOrEmpty(x.TableName));

        RuleFor(x => x.Severity)
            .MaximumLength(50).WithMessage("Severity en fazla 50 karakter olabilir")
            .When(x => !string.IsNullOrEmpty(x.Severity));

        RuleFor(x => x.Module)
            .MaximumLength(100).WithMessage("Module en fazla 100 karakter olabilir")
            .When(x => !string.IsNullOrEmpty(x.Module));

        RuleFor(x => x.IpAddress)
            .MaximumLength(50).WithMessage("IP address en fazla 50 karakter olabilir")
            .When(x => !string.IsNullOrEmpty(x.IpAddress));

        RuleFor(x => x)
            .Must(x => !x.StartDate.HasValue || !x.EndDate.HasValue || x.StartDate.Value <= x.EndDate.Value)
            .WithMessage("Start date, end date'den önce veya eşit olmalıdır")
            .When(x => x.StartDate.HasValue && x.EndDate.HasValue);
    }
}
