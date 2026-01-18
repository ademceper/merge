using Merge.Domain.ValueObjects;
namespace Merge.Application.DTOs.Analytics;

public record TwoFactorMethodCount(
    string Method,
    int Count,
    decimal Percentage
);

