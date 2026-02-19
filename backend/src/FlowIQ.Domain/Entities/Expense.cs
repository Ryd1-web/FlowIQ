using FlowIQ.Domain.Enums;

namespace FlowIQ.Domain.Entities;

public class Expense : BaseEntity
{
    public decimal Amount { get; set; }
    public ExpenseCategory Category { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; }
    public string? Notes { get; set; }

    // Foreign key
    public Guid BusinessId { get; set; }
    public Business Business { get; set; } = null!;
}
