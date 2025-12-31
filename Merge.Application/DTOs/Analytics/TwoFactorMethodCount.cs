namespace Merge.Application.DTOs.Analytics;

public class TwoFactorMethodCount
{
    public string Method { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal Percentage { get; set; }
}

