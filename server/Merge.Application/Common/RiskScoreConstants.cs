namespace Merge.Application.Common;

/// <summary>
/// ✅ HIGH-CQ-002 FIX: Magic numbers için constants - Risk score hesaplamalarında kullanılan sabitler
/// </summary>
public static class RiskScoreConstants
{
    // Order Risk Score Constants
    public const decimal HighValueOrderThreshold = 10000m;
    public const int HighValueOrderScore = 30;
    
    public const int NewUserDaysThreshold = 7;
    public const int NewUserScore = 20;
    
    public const int MultipleItemsThreshold = 10;
    public const int MultipleItemsScore = 15;
    
    public const int HighQuantityThreshold = 20;
    public const int HighQuantityScore = 15;
    
    public const int MaxRiskScore = 100;
    
    // Payment Fraud Prevention Constants
    public const decimal HighValuePaymentThreshold = 5000m;
    public const int HighValuePaymentScore = 25;
    
    public const int RecentPaymentsTimeWindowHours = 1;
    public const int RecentPaymentsThreshold = 3;
    public const int RecentPaymentsScore = 30;
    
    public const int MissingDeviceFingerprintScore = 15;
}
