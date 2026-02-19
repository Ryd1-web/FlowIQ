namespace FlowIQ.Domain.Entities;

public class User : BaseEntity
{
    public string PhoneNumber { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? OtpCode { get; set; }
    public DateTime? OtpExpiresAt { get; set; }
    public bool IsVerified { get; set; }
    public DateTime? LastLoginAt { get; set; }

    // Navigation
    public ICollection<Business> Businesses { get; set; } = new List<Business>();
}
