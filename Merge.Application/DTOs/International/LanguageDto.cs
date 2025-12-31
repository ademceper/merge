namespace Merge.Application.DTOs.International;

public class LanguageDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string NativeName { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; }
    public bool IsRTL { get; set; }
    public string FlagIcon { get; set; } = string.Empty;
}
