namespace FlowIQ.Domain.Entities;

public class Income : BaseEntity
{
    public decimal Amount { get; set; }
    public string Source { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; }
    public string? Notes { get; set; }

    // Foreign key
    public Guid BusinessId { get; set; }
    public Business Business { get; set; } = null!;
}
