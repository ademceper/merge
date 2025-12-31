using Merge.Domain.Entities;
namespace Merge.Application.DTOs.Identity;

public class TwoFactorSetupDto
{
    public TwoFactorMethod Method { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
}
