namespace Merge.Application.Common;

/// <summary>
/// Sensitive data masking helper for logging.
/// PII ve sensitive data'yı loglama için maskeleme sağlar.
/// </summary>
public static class LogMasking
{
    /// <summary>
    /// Email adresini maskeler.
    /// Örnek: "john.doe@example.com" → "jo***@example.com"
    /// </summary>
    public static string MaskEmail(string? email)
    {
        if (string.IsNullOrEmpty(email))
            return "[empty]";

        var atIndex = email.IndexOf('@');
        if (atIndex <= 0)
            return "***@***";

        if (atIndex <= 2)
            return "***@***";

        return email[..2] + "***" + email[(atIndex - 1)..];
    }

    /// <summary>
    /// Telefon numarasını maskeler.
    /// Örnek: "+905551234567" → "***4567"
    /// </summary>
    public static string MaskPhone(string? phone)
    {
        if (string.IsNullOrEmpty(phone) || phone.Length < 4)
            return "[masked]";

        return "***" + phone[^4..];
    }

    /// <summary>
    /// Kredi kartı numarasını maskeler.
    /// Örnek: "1234567890123456" → "1234 **** **** 3456"
    /// </summary>
    public static string MaskCardNumber(string? cardNumber)
    {
        if (string.IsNullOrEmpty(cardNumber) || cardNumber.Length < 8)
            return "[masked]";

        return cardNumber[..4] + " **** **** " + cardNumber[^4..];
    }

    /// <summary>
    /// Token'ı maskeler.
    /// Örnek: "abc123def456..." → "abc123de..."
    /// </summary>
    public static string MaskToken(string? token)
    {
        if (string.IsNullOrEmpty(token) || token.Length < 12)
            return "[masked]";

        return token[..8] + "...";
    }

    /// <summary>
    /// IP adresini maskeler (GDPR uyumlu).
    /// Örnek: "192.168.1.1" → "192.168.*.*"
    /// </summary>
    public static string MaskIpAddress(string? ipAddress)
    {
        if (string.IsNullOrEmpty(ipAddress))
            return "[empty]";

        var parts = ipAddress.Split('.');
        if (parts.Length == 4)
        {
            return $"{parts[0]}.{parts[1]}.*.*";
        }

        // IPv6 için
        if (ipAddress.Contains(':'))
        {
            var parts6 = ipAddress.Split(':');
            if (parts6.Length > 2)
            {
                return $"{parts6[0]}:{parts6[1]}:***";
            }
        }

        return "[masked]";
    }
}
