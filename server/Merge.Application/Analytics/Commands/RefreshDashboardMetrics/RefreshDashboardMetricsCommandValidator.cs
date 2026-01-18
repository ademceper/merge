using FluentValidation;

namespace Merge.Application.Analytics.Commands.RefreshDashboardMetrics;

public class RefreshDashboardMetricsCommandValidator : AbstractValidator<RefreshDashboardMetricsCommand>
{
    public RefreshDashboardMetricsCommandValidator()
    {
        // RefreshDashboardMetricsCommand parametre almadığı için validation gerekmez
        // Ancak FluentValidation pattern'i için boş validator oluşturuyoruz
    }
}

